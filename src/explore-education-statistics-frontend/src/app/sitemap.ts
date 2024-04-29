import getPublicationSlugs from '@frontend/services/sitemapService';
import { MetadataRoute } from 'next';

// Priority is a decimal bounded [0.0, 1.0] which indicates to Google which pages we most want users to find in search results.
// The default priority is 0.5.

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const publicationPages = await getPublicationSlugs();

  return [
    {
      url: process.env.PUBLIC_URL,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 1,
    },
    {
      url: `${process.env.PUBLIC_URL}not-found`,
      lastModified: new Date(),
      changeFrequency: 'never',
      priority: 0.0,
    },
    {
      url: `${process.env.PUBLIC_URL}contact-us`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.4,
    },
    {
      url: `${process.env.PUBLIC_URL}cookies`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.0,
    },
    {
      url: `${process.env.PUBLIC_URL}cookies/details`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.0,
    },
    {
      url: `${process.env.PUBLIC_URL}data-catalogue`,
      lastModified: new Date(),
      changeFrequency: 'daily',
      priority: 0.9,
    },
    // all routes matching:
    // data-catalogue/[publicationSlug]
    // data-tables/[publicationSlug]
    // find-statistics/[publication]
    ...publicationPages,
    {
      url: `${process.env.PUBLIC_URL}data-tables`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.8,
    },

    {
      url: `${process.env.PUBLIC_URL}find-statistics`,
      lastModified: new Date(),
      changeFrequency: 'always',
      priority: 0.9,
    },

    {
      url: `${process.env.PUBLIC_URL}glossary`,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 0.3,
    },
    {
      url: `${process.env.PUBLIC_URL}help-support`,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 0.3,
    },
    {
      url: `${process.env.PUBLIC_URL}methodology`,
      lastModified: new Date(),
      changeFrequency: 'monthly',
      priority: 0.7,
    },
    {
      url: `${process.env.PUBLIC_URL}privacy-notice`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.0,
    },
    // subscriptions - TODO: Check if this should even be accessible? Current yields a 404
    {
      url: `${process.env.PUBLIC_URL}subscriptions/verification-error`,
      lastModified: new Date(),
      changeFrequency: 'yearly',
      priority: 0.0,
    },
    // Omitting the following dynamic routes from the sitemap:
    // methodology/[methodology]
    // find-statistics/[publication]/[release]/data-guidance
    // find-statistics/[publication]/[release]/prerelease-access-list
    // find-statistics/[publication]/data-guidance
    // find-statistics/[publication]/prerelease-access-list
    // data-tables/fast-track/[dataBlockParentId]
    // data-tables/permalink/[publicationSlug]
    // data-catalogue/data-set/[dataSetFieldId]

    // Feels like we should add these, but that might require writing a new endpoint.
    // Currently would need to make a GET request PER publication
    // data-catalogue/[publicationSlug]/[releaseSlug]
    // data-tables/[publicationSlug]/[releaseSlug]
    // find-statistics/[publication]/[release]
  ];
}
