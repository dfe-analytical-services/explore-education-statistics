import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { ISitemapField } from 'next-sitemap';

export default async function getPublicationSlugs() {
  try {
    // TODO: Replace this with use of the publicationService, once the app router is more widely adopted
    // (using it currently requires more changes)
    const publications = (
      await (
        await fetch(
          // TODO: Create a more graceful way of opting out of pagination,
          // Or make multiple paginated calls, whatever best
          `${process.env.CONTENT_API_BASE_URL}/publications?pageSize=10000`,
          {
            method: 'GET',
            headers: {
              'Content-Type': 'application/json',
            },
          },
        )
      ).json<PaginatedList<PublicationListSummary>>()
    ).results;

    const routes: ISitemapField[] = publications.flatMap(publication => {
      return [
        {
          loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}`,
          lastmod: publication.published?.toISOString(),
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}`,
          lastmod: publication.published?.toISOString(),
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}`,
          lastmod: publication.published?.toISOString(),
          changefreq: 'monthly',
          priority: 0.6,
        },
      ];
    });
    return routes;
  } catch (err) {
    if (process.env.APP_ENV === 'Local') {
      // eslint-disable-next-line no-console
      console.error(
        'Encountered an error whilst trying to fetch publications to build the sitemap. This step requires that the Content API be running.',
      );
      return [];
    }
    throw err;
  }
}
