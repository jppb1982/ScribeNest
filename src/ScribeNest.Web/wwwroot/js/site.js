(function () {
  const storageKey = 'scribenest-theme';

  function applyTheme(theme) {
    const isDark = theme === 'dark';
    document.documentElement.classList.toggle('sn-dark', isDark);
    document.body.classList.toggle('sn-dark', isDark);

    const toggle = document.getElementById('themeToggle');
    if (toggle) {
      toggle.textContent = isDark ? 'Tema claro' : 'Tema oscuro';
      toggle.setAttribute('aria-pressed', String(isDark));
    }
  }

  function currentTheme() {
    return localStorage.getItem(storageKey) || 'light';
  }

  document.addEventListener('DOMContentLoaded', function () {
    applyTheme(currentTheme());

    const toggle = document.getElementById('themeToggle');
    if (toggle) {
      toggle.addEventListener('click', function () {
        const next = document.body.classList.contains('sn-dark') ? 'light' : 'dark';
        localStorage.setItem(storageKey, next);
        applyTheme(next);
      });
    }

    initSlugAutoFill();
    initMarkdownPreview();
    initAiMock();
    initDeleteConfirm();
  });


  function initSlugAutoFill() {
    const title = document.querySelector('[data-title-source]');
    const slug = document.querySelector('[data-slug-target]');
    if (!title || !slug) return;

    const sync = function () {
      slug.value = slugify(title.value || '');
      slug.dispatchEvent(new Event('input', { bubbles: true }));
    };

    title.addEventListener('input', sync);
    sync();
  }

  function slugify(value) {
    return String(value || '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .substring(0, 120);
  }

  function initMarkdownPreview() {
    const source = document.querySelector('[data-markdown-source]');
    const preview = document.querySelector('[data-markdown-preview]');
    if (!source || !preview) return;

    const render = function () {
      preview.innerHTML = markdownToHtml(source.value || '');
    };

    source.addEventListener('input', render);
    render();
  }

  function getSelectedCategoryText(category) {
    if (!category || !category.value) return '';

    const selected = category.selectedOptions && category.selectedOptions.length
      ? category.selectedOptions[0]
      : category.options[category.selectedIndex];

    return selected ? selected.textContent.trim() : '';
  }

  function initAiMock() {
    document.querySelectorAll('[data-ai-suggest]').forEach(function (button) {
      const form = button.closest('form') || document;
      const output = form.querySelector('[data-ai-output]');
      const title = form.querySelector('[data-title-source]');
      const content = form.querySelector('[data-markdown-source]');
      const tags = form.querySelector('[data-tags-target]');
      const category = form.querySelector('select[name="CategoryId"]');

      if (!output || !title || !content) return;

      button.addEventListener('click', async function () {
        output.classList.remove('d-none', 'alert-danger', 'alert-secondary', 'alert-success', 'alert-warning');

        const originalText = button.textContent;
        button.disabled = true;
        button.textContent = 'Generando...';
        output.classList.add('alert-secondary');
        output.textContent = 'Consultando IA mock local...';

        try {
          const response = await fetch('/api/ai-assistant/suggestions', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json'
            },
            body: JSON.stringify({
              title: title.value || '',
              content: content.value || '',
              category: getSelectedCategoryText(category),
              tags: tags ? tags.value || '' : ''
            })
          });

          if (!response.ok) {
            const message = await response.text();
            throw new Error(message || `HTTP ${response.status}`);
          }

          const data = await response.json();
          const suggestedTags = Array.isArray(data.suggestedTags) ? data.suggestedTags : [];
          const warnings = Array.isArray(data.warnings) ? data.warnings : [];
          const currentTags = tags ? (tags.value || '').trim() : '';
          const editorialNotes = Array.isArray(data.editorialNotes) ? data.editorialNotes : warnings;
          const consistency = data.editorialConsistency || (warnings.length ? 'Media' : 'Alta');
          const consistencyHtml = `<div class="ai-editorial-consistency"><strong>Consistencia editorial:</strong> ${escapeHtml(consistency)}</div>`;
          const notesHtml = editorialNotes.length
            ? `<div class="ai-warnings"><strong>Observaciones:</strong><ul>${editorialNotes.map(function (note) { return `<li>${escapeHtml(note)}</li>`; }).join('')}</ul></div>`
            : '';

          output.classList.remove('alert-secondary');
          output.classList.add(warnings.length ? 'alert-warning' : 'alert-success');
          output.innerHTML = [
            '<strong>IA mock local:</strong>',
            consistencyHtml,
            notesHtml,
            `<div><strong>Resumen:</strong> ${escapeHtml(data.summary || '')}</div>`,
            `<div><strong>Excerpt:</strong> ${escapeHtml(data.excerpt || '')}</div>`,
            currentTags ? `<div><strong>Tags actuales:</strong> ${escapeHtml(currentTags)}</div>` : '',
            `<div><strong>Tags sugeridos por IA:</strong> ${escapeHtml(suggestedTags.join(', '))}</div>`,
            '<div class="small mt-1">La IA no modifica los tags automaticamente. Revisalos y copialos manualmente si queres usarlos.</div>',
            `<div><strong>Explain for juniors:</strong> ${escapeHtml(data.explainForJuniors || '')}</div>`
          ].filter(Boolean).join('');
        } catch (error) {
          output.classList.remove('alert-secondary');
          output.classList.add('alert-danger');
          output.textContent = error.message || 'No se pudieron generar sugerencias.';
        } finally {
          button.disabled = false;
          button.textContent = originalText;
        }
      });
    });
  }

  function initDeleteConfirm() {
    document.querySelectorAll('[data-confirm-delete]').forEach(function (form) {
      form.addEventListener('submit', function (event) {
        const title = form.getAttribute('data-delete-title') || 'este artículo';
        if (!confirm(`¿Confirmás eliminar "${title}"? Esta acción no se puede deshacer.`)) {
          event.preventDefault();
        }
      });
    });
  }

  function markdownToHtml(markdown) {
    const normalized = String(markdown || '')
      .replace(/\r\n/g, '\n')
      .replace(/\r/g, '\n');

    const lines = normalized.split('\n');
    let html = '';
    let inList = false;

    for (const raw of lines) {
      const line = raw.trim();

      if (!line) {
        if (inList) {
          html += '</ul>';
          inList = false;
        }
        continue;
      }

      if (line.startsWith('- ')) {
        if (!inList) {
          html += '<ul>';
          inList = true;
        }
        html += `<li>${inline(line.slice(2))}</li>`;
        continue;
      }

      if (inList) {
        html += '</ul>';
        inList = false;
      }

      if (line.startsWith('### ')) html += `<h3>${inline(line.slice(4))}</h3>`;
      else if (line.startsWith('## ')) html += `<h2>${inline(line.slice(3))}</h2>`;
      else if (line.startsWith('# ')) html += `<h1>${inline(line.slice(2))}</h1>`;
      else html += `<p>${inline(line)}</p>`;
    }

    if (inList) html += '</ul>';
    return html || '<p class="text-muted">Escribí contenido para ver la vista previa.</p>';
  }

  function inline(value) {
    return escapeHtml(value)
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/`(.+?)`/g, '<code>$1</code>');
  }

  function normalize(value) {
    return (value || '').replace(/\s+/g, ' ').trim();
  }

  function buildTags(value) {
    const text = value.toLowerCase();
    const tags = [];
    const add = (keyword, tag) => {
      if (text.includes(keyword) && !tags.includes(tag)) tags.push(tag);
    };

    add('.net', '.NET');
    add('asp.net', 'ASP.NET Core');
    add('angular', 'Angular');
    add('api', 'REST API');
    add('entity framework', 'EF Core');
    add('ef core', 'EF Core');
    add('architecture', 'Architecture');
    add('repository', 'Repository Pattern');
    add('data', 'Data');
    add('dashboard', 'Dashboard');
    add('ai', 'AI');

    return tags.length ? tags.slice(0, 6) : ['Technical Writing', 'Software Development'];
  }

  function explainForJuniors(text) {
    const first = text.split('.').find(Boolean) || text;
    return `En simple: ${first.trim()}. La clave es identificar qué problema resuelve y qué trade-off introduce.`;
  }

  function escapeHtml(value) {
    return String(value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
})();
