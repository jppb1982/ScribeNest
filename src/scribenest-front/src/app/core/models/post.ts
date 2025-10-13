export interface PostListItem {
  id: number;
  title: string;
  slug: string;
  category: string;
  publishedAt: string;  
}

export interface PostDetail extends PostListItem {
  content: string;
}
