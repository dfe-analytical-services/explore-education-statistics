import { ScriptMetaItem } from '@frontend/components/PageMetaItem';
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

  const dataSetFileMeta = {
    '@context': [
      'https://schema.org ',
      { csvw: 'https://www.w3.org/ns/csvw ' },
    ],
    '@type': 'Dataset',
    name: `${release.publication.title} - ${title}`,
    alternateName: [title, file.name],
    description: summary,
    url: `https://explore-education-statistics.service.gov.uk/data-catalogue/data-set/${id}`,
    sameAs: `https://explore-education-statistics.service.gov.uk/data-catalogue/data-set/${id}`,
    isAccessibleForFree: true,
    license:
      'https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/ ',
    includedInDataCatalog: {
      '@type': 'DataCatalog',
      name: 'Explore education statistics datasets',
      description:
        'A http://GOV.UK  Department for Education repository for education statistics',
      url: 'https://explore-education-statistics.service.gov.uk/data-catalogue ',
    },
    creator: [
      {
        '@type': 'GovernmentOrganization',
        name: 'Department for Education',
        description:
          'The Department for Education is responsible for children’s services and education, including early years, schools, higher and further education policy, apprenticeships and wider skills in England.',
        url: 'https://www.gov.uk/government/organisations/department-for-education ',
        sameAs: 'https://ror.org/0320bge18 ',
      },
      //   {
      //     '@type': 'Person',
      //     name: release.publication.Contact.ContactName,
      //     telephone: Publication.Contact.ContactTelNo,
      //     email: Publication.Contact.TeamEmail,
      //   },
    ],
    publisher: {
      '@type': 'GovernmentOrganization',
      name: 'Department for Education',
      description:
        'The Department for Education is responsible for children’s services and education, including early years, schools, higher and further education policy, apprenticeships and wider skills in England.',
      url: 'https://www.gov.uk/government/organisations/department-for-education ',
      sameAs: 'https://ror.org/0320bge18 ',
    },
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
