import methodologyService, {
  MethodologySitemapSummary,
} from '@common/services/methodologyService';
import publicationService, {
  PublicationSitemapSummary,
} from '@common/services/publicationService';
import dataSetService, {
  DataSetSitemapSummary,
} from '@common/services/dataSetService';
import { ISitemapField } from 'next-sitemap';

export default async function getSitemapFields(): Promise<ISitemapField[]> {
  const methodologies = await methodologyService.getSitemapSummaries();
  const publications = await publicationService.getSitemapSummaries();
  const dataSets = await dataSetService.getSitemapSummaries();

  const sitemapFields: ISitemapField[] = [
    // matches dynamic routes following the patterns:
    //  methodology/[methodology]
    ...buildMethodologyRoutes(methodologies),

    // matches dynamic routes following the patterns:
    //  data-catalogue/[publicationSlug]
    //  data-tables/[publicationSlug]
    //  find-statistics/[publication]
    //  find-statistics/[publication]/data-guidance
    //  find-statistics/[publication]/prerelease-access-list
    //  find-statistics/[publication]/[release]/data-guidance
    //  find-statistics/[publication]/[release]/prerelease-access-list
    //  data-catalogue/[publicationSlug]/[releaseSlug]
    //  data-tables/[publicationSlug]/[releaseSlug]
    //  find-statistics/[publication]/[release]
    ...buildPublicationRoutes(publications),

    // matches dynamic routes following the patterns:
    //  data-catalogue/data-set/[dataSetFieldId]
    ...buildDataSetRoutes(dataSets),

    // Other routes which exist on the site but are excluded from the sitemap by config:
    //  data-tables/fast-track/[dataBlockParentId]
    //  data-tables/permalink/[publicationSlug]
    //
  ];

  return sitemapFields;
}

function buildMethodologyRoutes(
  methodologies: MethodologySitemapSummary[],
): ISitemapField[] {
  return methodologies.map(methodology => ({
    loc: `${process.env.PUBLIC_URL}methodology/${methodology.slug}`,
    lastmod: `${methodology.lastModified}`,
    changefreq: 'monthly',
    priority: 0.6,
  }));
}

function buildPublicationRoutes(
  publications: PublicationSitemapSummary[],
): ISitemapField[] {
  let fields: ISitemapField[] = [];

  publications.forEach(publication => {
    fields = fields.concat([
      {
        loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}`,
        lastmod: `${publication.lastModified}`,
        changefreq: 'monthly',
        priority: 0.6,
      },
      {
        loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}`,
        lastmod: `${publication.lastModified}`,
        changefreq: 'monthly',
        priority: 0.6,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}`,
        lastmod: `${publication.lastModified}`,
        changefreq: 'monthly',
        priority: 0.6,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/data-guidance`,
        lastmod: `${publication.lastModified}`,
        changefreq: 'monthly',
        priority: 0.6,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/prerelease-access-list`,
        lastmod: `${publication.lastModified}`,
        changefreq: 'monthly',
        priority: 0.6,
      },
    ]);

    publication.releases.forEach(release => {
      fields = fields.concat([
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}`,
          lastmod: `${release.lastModified}`,
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}/data-guidance`,
          lastmod: `${release.lastModified}`,
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}/prerelease-access-list`,
          lastmod: `${release.lastModified}`,
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}/${release.slug}`,
          lastmod: `${release.lastModified}`,
          changefreq: 'monthly',
          priority: 0.6,
        },
        {
          loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}/${release.slug}`,
          lastmod: `${release.lastModified}`,
          changefreq: 'monthly',
          priority: 0.6,
        },
      ]);
    });
  });
  return fields;
}

function buildDataSetRoutes(
  dataSets: DataSetSitemapSummary[],
): ISitemapField[] {
  return dataSets.map(dataSet => ({
    loc: `${process.env.PUBLIC_URL}data-catalogue/data-set/${dataSet.id}`,
    lastmod: `${dataSet.lastModified}`,
    changefreq: 'monthly',
    priority: 0.6,
  }));
}
