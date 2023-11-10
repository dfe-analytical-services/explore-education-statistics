export interface Redirects {
  methodologies: Redirect[];
  publications: Redirect[];
}

export type RedirectType = keyof Redirects;

interface Redirect {
  fromSlug: string;
  toSlug: string;
}

const contentApiUrl = process.env.CONTENT_API_BASE_URL;

const redirectService = {
  async list(): Promise<Redirects> {
    return (await fetch(`${contentApiUrl}/redirects`)).json();
  },
};

export default redirectService;
