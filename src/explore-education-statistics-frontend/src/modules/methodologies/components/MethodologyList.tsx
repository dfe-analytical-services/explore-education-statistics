import Link from '@frontend/components/Link';
import React from 'react';
import { Publication } from '@common/services/publicationService';

interface Props {
  publications: Publication[];
}

function MethodologyList({ publications }: Props) {
  return (
    <ul className="govuk-list govuk-list--bullet">
      {publications.length > 0 ? (
        publications.map(({ title, slug }) => (
          <>
            <h3 className="govuk-heading-s govuk-!-margin-bottom-0">{title}</h3>
            <div className="govuk-!-margin-bottom-1">
              <Link to={`/methodology/${slug}`}>
                View methodology
                <span className="govuk-visually-hidden">for {title}</span>
              </Link>
            </div>
          </>
        ))
      ) : (
        <></>
      )}
    </ul>
  );
}

export default MethodologyList;
