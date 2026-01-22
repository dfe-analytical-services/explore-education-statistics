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
import educationInNumbersService, {
  EinPageSitemapItem,
} from '@frontend/services/educationInNumbersService';

export default async function getSitemapFields(): Promise<ISitemapField[]> {
  const methodologies = await methodologyService.listSitemapItems();
  const publications = await publicationService.listSitemapItems();
  const dataSets = await dataSetService.listSitemapItems();
  // const einPages = await educationInNumbersService.listSitemapItems(); // TODO: EES-6497

  return [
    ...buildMethodologyRoutes(methodologies),
    ...buildPublicationRoutes(publications),
    ...buildDataSetRoutes(dataSets),
    // ...buildEinPageRoutes(einPages), // TODO: EES-6497
  ];
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
          loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/releases`,
          lastmod: publication.lastModified,
          priority: 0.4,
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
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/explore`,
            lastmod: release.lastModified,
          },
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/methodology`,
            lastmod: release.lastModified,
          },
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/help`,
            lastmod: release.lastModified,
          },
          {
            loc: `${process.env.PROD_PUBLIC_URL}/find-statistics/${publication.slug}/${release.slug}/updates`,
            lastmod: release.lastModified,
            priority: 0.4,
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

function buildEinPageRoutes(einPages: EinPageSitemapItem[]): ISitemapField[] {
  return einPages.map(page => ({
    loc: `${process.env.PROD_PUBLIC_URL}/education-in-numbers/${page.slug}`,
    lastmod: page.lastModified,
  }));
}
