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
    <ul data-testid="methodology-list">
      {filteredPublications.map(publication => (
        <>
          {publication.methodologies.map(methodology => (
            <li key={methodology.id} className="govuk-!-margin-bottom-2">
              <Link to={`/methodology/${methodology.slug}`}>
                {methodology.title}
              </Link>
            </li>
          ))}
        </>
      ))}
    </ul>
  );
}

export default MethodologyList;
