export interface PostListItem {
  id: number;
  title: string;
  slug: string;
  excerpt: string;
  categoryId: number;
  category: string;
  publishedAt: string;
  tags: string[];
}

export interface PostDetail extends PostListItem {
  content: string;
}

export interface PostUpsert {
  title: string;
  slug: string;
  content: string;
  tags: string;
  categoryId: number | null;
}

export interface AiSuggestionRequest {
  title: string;
  content: string;
  category?: string;
  tags?: string;
}

export interface AiSuggestionResponse {
  suggestedTitle: string;
  summary: string;
  excerpt: string;
  suggestedTags: string[];
  explainForJuniors: string;
  warnings: string[];
  editorialConsistency: string;
  editorialNotes: string[];
}
