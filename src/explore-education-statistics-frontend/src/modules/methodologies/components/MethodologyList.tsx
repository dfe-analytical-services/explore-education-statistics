import React from 'react';
import { Publication } from '@common/services/publicationService';
import Link from '@frontend/components/Link';

interface Props {
  publications: Publication[];
}

function MethodologyList({ publications }: Props) {
  return (
    <ul className="govuk-list govuk-list--bullet">
      {publications.map(publication => {
        return (
          <>
            {publication.methodology && (
              <li className="govuk-!-margin-bottom-2" key={publication.id}>
                <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                  {publication.title}
                </h3>
                <div>
                  <Link to={`/methodology/${publication.methodology.slug}`}>
                    View methodology
                  </Link>
                </div>
              </li>
            )}
          </>
        );
      })}
    </ul>
  );
}

export default MethodologyList;
