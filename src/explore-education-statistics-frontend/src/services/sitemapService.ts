'use server';

import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { MetadataRoute } from 'next';

export default async function getPublicationSlugs() {
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

  const routes: MetadataRoute.Sitemap = publications.flatMap(publication => {
    return [
      {
        url: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}`,
        lastModified: publication.published ?? new Date(),
        changeFrequency: 'monthly',
        priority: 0.6,
      },
      {
        url: `${process.env.PUBLIC_URL}data-tables/${publication.slug}`,
        lastModified: publication.published ?? new Date(),
        changeFrequency: 'monthly',
        priority: 0.6,
      },
      {
        url: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}`,
        lastModified: publication.published ?? new Date(),
        changeFrequency: 'monthly',
        priority: 0.6,
      },
    ];
  });
  return routes;
}
