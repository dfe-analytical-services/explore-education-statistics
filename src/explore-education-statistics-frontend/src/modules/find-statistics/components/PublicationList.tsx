import { PublicationSummary } from '@common/services/themeService';
import {
  PublicationType,
  publicationTypes,
} from '@common/services/types/publicationType';
import Link from '@frontend/components/Link';
import groupBy from 'lodash/groupBy';
import React from 'react';

interface Props {
  publications: PublicationSummary[];
}

const publicationTypeReferenceOrder: PublicationType[] = [
  'NationalAndOfficial',
  'Experimental',
  'AdHoc',
  'ManagementInformation',
  'Legacy',
];

const groupPublicationsByType = (publications: PublicationSummary[]) =>
  Object.entries(groupBy(publications, publication => publication.type))
    .map(([key, value]) => ({
      type: key as PublicationType,
      group: value,
    }))
    .sort(
      ({ type: a }, { type: b }) =>
        publicationTypeReferenceOrder.indexOf(a) -
        publicationTypeReferenceOrder.indexOf(b),
    );

function PublicationList({ publications }: Props) {
  return (
    <>
      {groupPublicationsByType(publications).map(({ type, group }) => (
        <div key={type} data-testid="publication-type">
          <h3 data-testid={`type-heading-${type}`} className="govuk-heading-s">
            {publicationTypes[type]}
          </h3>
          <ul className="govuk-!-margin-top-0">
            {group.map(({ id, legacyPublicationUrl, slug, title }) => (
              <li key={id} className="govuk-!-margin-bottom-0">
                <h4
                  id={`publication-heading-${id}`}
                  className="govuk-heading-s govuk-!-font-weight-regular govuk-!-margin-bottom-0"
                >
                  {title}
                </h4>
                {legacyPublicationUrl ? (
                  <div className="govuk-!-margin-bottom-3">
                    {' '}
                    Currently available via{' '}
                    <a href={legacyPublicationUrl}>
                      Statistics at DfE{' '}
                      <span className="govuk-visually-hidden">for {title}</span>
                    </a>
                  </div>
                ) : (
                  <div className="govuk-grid-row govuk-!-margin-bottom-3">
                    <div className="govuk-grid-column-one-third govuk-!-margin-bottom-1">
                      <Link
                        to={`/find-statistics/${slug}`}
                        testId={`View stats link for ${title}`}
                      >
                        View statistics and data{' '}
                        <span className="govuk-visually-hidden">
                          for {title}
                        </span>
                      </Link>
                    </div>

                    <div className="govuk-grid-column-one-third">
                      <Link
                        to={`/data-tables/${slug}`}
                        data-testid={`Create table link for ${title}`}
                      >
                        Create your own tables{' '}
                        <span className="govuk-visually-hidden">
                          for {title}
                        </span>
                      </Link>
                    </div>
                  </div>
                )}
              </li>
            ))}
          </ul>
        </div>
      ))}
    </>
  );
}

export default PublicationList;
