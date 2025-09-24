using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ScribeNest.Application.Interfaces;
using ScribeNest.Domain.Entities;
using ScribeNest.Web.Models;

public class PostsController(IUnitOfWork uow) : Controller
{
    private readonly IUnitOfWork _uow = uow;

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var posts = await _uow.Posts.ListAsync(p =>
            string.IsNullOrWhiteSpace(q)
            || p.Title.Contains(q)
            || p.Content.Contains(q));

        return View(posts.OrderByDescending(p => p.PublishedAt));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post is null) return NotFound();
        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var cats = await _uow.Categories.ListAsync();
        var vm = new PostCreateVm
        {
            Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PostCreateVm vm)
    {
        if (!ModelState.IsValid)
        {
            var cats = await _uow.Categories.ListAsync();
            vm.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(vm);
        }

        var post = new Post
        {
            Title = vm.Title,
            Slug = vm.Slug,
            Content = vm.Content,
            CategoryId = vm.CategoryId,
            PublishedAt = DateTime.UtcNow
        };

        await _uow.Posts.AddAsync(post);
        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
        
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _uow.Posts.GetByIdAsync(id);
        if (p is null) return NotFound();

        var cats = await _uow.Categories.ListAsync();
        var vm = new PostEditVm
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Content = p.Content,
            CategoryId = p.CategoryId,
            Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PostEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            var cats = await _uow.Categories.ListAsync();
            vm.Categories = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
            return View(vm);
        }

        var p = await _uow.Posts.GetByIdAsync(vm.Id);
        if (p is null) return NotFound();

        p.Title = vm.Title;
        p.Slug = vm.Slug;
        p.Content = vm.Content;
        p.CategoryId = vm.CategoryId;

        await _uow.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _uow.Posts.GetByIdAsync(id);
        if (p is null) return NotFound();
        return View(p);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _uow.Posts.GetByIdAsync(id);
        if (post is null) return NotFound();

        _uow.Posts.Delete(post);
        await _uow.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
