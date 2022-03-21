export interface GlossaryEntry {
  title: string;
  slug: string;
  body: string;
}

export interface GlossaryCategory {
  heading: string;
  entries: GlossaryEntry[];
}
