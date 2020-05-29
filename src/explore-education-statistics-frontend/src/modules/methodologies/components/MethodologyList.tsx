import React from 'react';
import { Publication } from '@common/services/publicationService';
import Link from '@frontend/components/Link';

interface Props {
  publications: Publication[];
}

function MethodologyList({ publications }: Props) {
  const filteredPublications = publications.filter(
    publication => !!publication.methodology,
  );

  if (!filteredPublications.length) {
    return null;
  }

  return (
    <ul>
      {filteredPublications.map(publication => (
        <li className="govuk-!-margin-bottom-2" key={publication.id}>
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            {publication.title}
          </h3>

          <Link
            to="/methodology/[methodology]"
            as={`/methodology/${publication.methodology?.slug}`}
          >
            View methodology
          </Link>
        </li>
      ))}
    </ul>
  );
}

export default MethodologyList;
