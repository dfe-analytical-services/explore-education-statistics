import sanitizeHtml from '@common/utils/sanitizeHtml';
import { ScriptMetaItem } from '@frontend/components/PageMetaItem';
import getCurrentTenant from '@frontend/utils/getCurrentTenant';
import { DataSetFile } from '@frontend/services/dataSetFileService';

interface Props {
  dataSetFile: DataSetFile;
}

/**
 * Used to set CSVW (CSV on the Web) meta data to help make datasets more findable in terms of SEO
 */
export default function getDataSetFileMetaCSVW({
  dataSetFile,
}: Props): ScriptMetaItem {
  const { file, release, summary, title, id } = dataSetFile;
  const { service, organisation } = getCurrentTenant();
  const dataSetUrl = `${service.productionUrl}/data-catalogue/data-set/${id}`;
  const organisationMeta = {
    '@type': 'Organization',
    name: organisation.name,
    description: organisation.description,
    url: organisation.url,
    ...(organisation.rorUrl && { sameAs: organisation.rorUrl }),
  };

  const dataSetFileMeta = {
    '@context': ['https://schema.org', { csvw: 'https://www.w3.org/ns/csvw' }],
    '@type': 'Dataset',
    name: `${release.publication.title} - ${title}`,
    alternateName: [title, file.name],
    description:
      summary.length < 50
        ? `${
            summary.length ? `${summary} - ` : ''
          }${service.nameStartSentence} data set ${title} from ${
            release.publication.title
          }`
        : sanitizeHtml(summary, { allowedTags: [] }),
    url: dataSetUrl,
    sameAs: dataSetUrl,
    isAccessibleForFree: true,
    license:
      'https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/ ',
    includedInDataCatalog: {
      '@type': 'DataCatalog',
      name: `${service.nameStartSentence} datasets`,
      description: `A ${organisation.name} repository for ${service.subjectMidSentence} statistics`,
      url: `${service.productionUrl}/data-catalogue`,
    },
    creator: [
      organisationMeta,
      //   {
      //     '@type': 'Person',
      //     name: release.publication.Contact.ContactName,
      //     telephone: Publication.Contact.ContactTelNo,
      //     email: Publication.Contact.TeamEmail,
      //   },
    ],
    publisher: organisationMeta,
    temporalCoverage: `${file.meta.timePeriodRange.from}/${file.meta.timePeriodRange.to}`,
    mainEntity: !file.dataCsvPreview
      ? undefined
      : {
          '@type': 'csvw:Table',
          'csvw:tableSchema': {
            'csvw:columns': file.dataCsvPreview.headers.map((header, index) => {
              return {
                'csvw:name': header,
                'csvw:cells': file.dataCsvPreview.rows.map(row => {
                  return {
                    'csvw:value': row[index],
                  };
                }),
              };
            }),
          },
        },
  };

  return {
    type: 'script',
    attributes: { type: 'application/ld+json' },
    content: JSON.stringify(dataSetFileMeta),
  };
}
