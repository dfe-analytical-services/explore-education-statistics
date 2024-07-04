import createMemoryCache from '@common/utils/cache/createMemoryCache';
import { Redirects } from '@common/utils/url/applyRedirectRules';

const redirects = createMemoryCache<Redirects>({
  get: async () => (await fetch(`${contentApiUrl}/redirects`)).json(),
  initial: { publications: [], methodologies: [] },
});

const contentApiUrl = process.env.CONTENT_API_BASE_URL;

const redirectService = {
  async list(): Promise<Redirects> {
    return redirects.get();
  },
};

export default redirectService;
