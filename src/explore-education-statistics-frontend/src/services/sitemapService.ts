import methodologyService, {
  MethodologySitemapItem,
} from '@common/services/methodologyService';
import publicationService, {
  PublicationSitemapItem,
} from '@common/services/publicationService';
// import dataSetService, {
//   DataSetSitemapItem,
// } from '@common/services/dataSetService';
import { ISitemapField } from 'next-sitemap';

export default async function getSitemapFields(): Promise<ISitemapField[]> {
  const methodologies = await methodologyService.getSitemapItems();
  const publications = await publicationService.getSitemapItems();
  // TODO: Add datasets to sitemap once this work has been completed
  // const dataSets = await dataSetService.getSitemapItems();

  const sitemapFields: ISitemapField[] = [
    ...buildMethodologyRoutes(methodologies),
    ...buildPublicationRoutes(publications),
    // ...buildDataSetRoutes(dataSets),
  ];

  return sitemapFields;
}

function buildMethodologyRoutes(
  methodologies: MethodologySitemapItem[],
): ISitemapField[] {
  return methodologies.map(methodology => ({
    loc: `${process.env.PUBLIC_URL}methodology/${methodology.slug}`,
    lastmod: methodology.lastModified,
  }));
}

function buildPublicationRoutes(
  publications: PublicationSitemapItem[],
): ISitemapField[] {
  let fields: ISitemapField[] = [];

  publications.forEach(publication => {
    fields = fields.concat([
      {
        loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}`,
        lastmod: publication.lastModified,
      },
      {
        loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}`,
        lastmod: publication.lastModified,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}`,
        lastmod: publication.lastModified,
        changefreq: 'monthly',
        priority: 0.7,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/data-guidance`,
        lastmod: publication.lastModified,
        priority: 0.4,
      },
      {
        loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/prerelease-access-list`,
        lastmod: publication.lastModified,
        priority: 0.2,
      },
    ]);

    publication.releases.forEach(release => {
      fields = fields.concat([
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}`,
          lastmod: release.lastModified,
        },
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}/data-guidance`,
          lastmod: release.lastModified,
          priority: 0.4,
        },
        {
          loc: `${process.env.PUBLIC_URL}find-statistics/${publication.slug}/${release.slug}/prerelease-access-list`,
          lastmod: release.lastModified,
          priority: 0.2,
        },
        {
          loc: `${process.env.PUBLIC_URL}data-catalogue/${publication.slug}/${release.slug}`,
          lastmod: release.lastModified,
        },
        {
          loc: `${process.env.PUBLIC_URL}data-tables/${publication.slug}/${release.slug}`,
          lastmod: release.lastModified,
        },
      ]);
    });
  });
  return fields;
}

// function buildDataSetRoutes(
//   dataSets: DataSetSitemapItem[],
// ): ISitemapField[] {
//   return dataSets.map(dataSet => ({
//     loc: `${process.env.PUBLIC_URL}data-catalogue/data-set/${dataSet.id}`,
//     lastmod: dataSet.lastModified ? `${dataSet.lastModified}` : undefined,
//   }));
// }
