import methodologyService, {
  MethodologySitemapItem,
} from '@common/services/methodologyService';
import publicationService, {
  PublicationSitemapItem,
} from '@common/services/publicationService';
import dataSetService, {
  DataSetSitemapItem,
} from '@common/services/dataSetService';
import { ISitemapField } from 'next-sitemap';

export default async function getSitemapFields(): Promise<ISitemapField[]> {
  const methodologies = await methodologyService.listSitemapItems();
  const publications = await publicationService.listSitemapItems();
  const dataSets = await dataSetService.listSitemapItems();

  const sitemapFields: ISitemapField[] = [
    ...buildMethodologyRoutes(methodologies),
    ...buildPublicationRoutes(publications),
    ...buildDataSetRoutes(dataSets),
  ];

  return sitemapFields;
}

function buildMethodologyRoutes(
  methodologies: MethodologySitemapItem[],
): ISitemapField[] {
  return methodologies.map(methodology => ({
    loc: `${process.env.PROD_PUBLIC_URL}/methodology/${methodology.slug}`,
    lastmod: methodology.lastModified,
  }));
}

function buildPublicationRoutes(
  publications: PublicationSitemapItem[],
): ISitemapField[] {
  const fields: ISitemapField[] = [];

  publications.forEach(publication => {
    fields.push(
      ...[
        // TODO: Check if data-tables should be included in the sitemap
        // Add <noindex, nofollow> to the data-tables page if not
        // Add to robots.txt if not
        // {
        //   loc: `${process.env.PROD_PUBLIC_URL}/data-tables/${publication.slug}`,
        //   lastmod: publication.lastModified,
        // },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}`,
          lastmod: publication.lastModified,
          priority: 0.7,
        },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/data-guidance`,
          lastmod: publication.lastModified,
          priority: 0.4,
        },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/prerelease-access-list`,
          lastmod: publication.lastModified,
          priority: 0.2,
        },
      ],
    );

    fields.push(
      ...[
        // TODO: Check if data-tables should be included in the sitemap
        // Add <noindex, nofollow> to the data-tables page if not
        // Add to robots.txt if not
        // {
        //   loc: `${process.env.PROD_PUBLIC_URL}/data-tables/${publication.slug}`,
        //   lastmod: publication.lastModified,
        // },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}`,
          lastmod: publication.lastModified,
          priority: 0.7,
        },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/data-guidance`,
          lastmod: publication.lastModified,
          priority: 0.4,
        },
        {
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/prerelease-access-list`,
          lastmod: publication.lastModified,
          priority: 0.2,
        },
      ],
    );

    publication.releases.forEach(release => {
      fields.push(
        ...[
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}`,
            lastmod: release.lastModified,
          },
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/data-guidance`,
            lastmod: release.lastModified,
            priority: 0.4,
          },
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/prerelease-access-list`,
            lastmod: release.lastModified,
            priority: 0.2,
          },
          // TODO: Check if data-tables should be included in the sitemap
          // Add <noindex, nofollow> to the data-tables page if not
          // Add to robots.txt if not
          // {
          //   loc: `${process.env.PROD_PUBLIC_URL}/data-tables/${publication.slug}/${release.slug}`,
          //   lastmod: release.lastModified,
          // },
        ],
      );
    });
  });
  return fields;
}

function buildDataSetRoutes(dataSets: DataSetSitemapItem[]): ISitemapField[] {
  return dataSets.map(dataSet => ({
    loc: `${
      process.env.PROD_PUBLIC_URL
    }/data-catalogue/data-set/${dataSet.id.toLowerCase()}`,
    lastmod: dataSet.lastModified,
  }));
}
