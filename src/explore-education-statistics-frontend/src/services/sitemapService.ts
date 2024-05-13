import methodologyService from '@common/services/methodologyService';
import publicationService from '@common/services/publicationService';
import { ISitemapField } from 'next-sitemap';

export async function getMethodologySitemapFields(): Promise<ISitemapField[]> {
  try {
    const methodologySummaries = await methodologyService.getSitemapSummaries();

    const routes: ISitemapField[] = methodologySummaries.map(methodology => ({
      loc: `${process.env.PUBLIC_URL}methodology/${methodology.slug}`,
      lastmod: `${methodology.lastModified}`,
      changefreq: 'monthly',
      priority: 0.6,
    }));
    return routes;
  } catch (err) {
    if (process.env.APP_ENV === 'Local') {
      // eslint-disable-next-line no-console
      console.error(
        'Encountered an error whilst trying to fetch methodologies to build the sitemap. This step requires that the Content API be running.',
      );
      return [];
    }
    throw err;
  }
}

// Need to match:
//  data-catalogue/[publicationSlug]
//  data-tables/[publicationSlug]
//  find-statistics/[publication]
//  methodology/[methodology] !!!!!!!! Currently can be either methodology
//  find-statistics/[publication]/[release]/data-guidance
//  find-statistics/[publication]/[release]/prerelease-access-list
//  find-statistics/[publication]/data-guidance
//  find-statistics/[publication]/prerelease-access-list
//  data-tables/fast-track/[dataBlockParentId]
//  data-tables/permalink/[publicationSlug]
//  data-catalogue/data-set/[dataSetFieldId]
//  Feels like we should add these, but that might require writing a new endpoint.
//  Currently would need to make a GET request PER publication
//  data-catalogue/[publicationSlug]/[releaseSlug]
//  data-tables/[publicationSlug]/[releaseSlug]
//  find-statistics/[publication]/[release]

export default async function getPublicationSlugs() {
  try {
    const publications = await publicationService.listPublications({
      pageSize: 10000,
    });

    const routes: ISitemapField[] = publications.results.flatMap(
      publication => {
        return [
          {
            loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}`,
            lastmod: `${publication.published}`,
            changefreq: 'monthly',
            priority: 0.6,
          },
          {
            loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}`,
            lastmod: `${publication.published}`,
            changefreq: 'monthly',
            priority: 0.6,
          },
          {
            loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}`,
            lastmod: `${publication.published}`,
            changefreq: 'monthly',
            priority: 0.6,
          },
        ];
      },
    );
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
