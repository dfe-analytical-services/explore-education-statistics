import { PublicationMethodologySummary } from '@common/services/themeService';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publications: PublicationMethodologySummary[];
}

function MethodologyList({ publications }: Props) {
  const filteredPublications = publications.filter(
    publication => !!publication.methodologies,
  );

  if (!filteredPublications.length) {
    return null;
  }

  return (
    <ul>
      {filteredPublications.map(publication => (
        <li className="govuk-!-margin-bottom-2" key={publication.id}>
          <h3
            className="govuk-heading-s govuk-!-margin-bottom-0"
            id={`methodology-heading-${publication.id}`}
          >
            {publication.title}
          </h3>

          <ul>
            {publication.methodologies.map(methodology => (
              <li key={methodology.id}>
                <Link to={`/methodology/${methodology.slug}`}>
                  {methodology.title}
                </Link>
              </li>
            ))}
          </ul>
        </li>
      ))}
    </ul>
  );
}

export default MethodologyList;
