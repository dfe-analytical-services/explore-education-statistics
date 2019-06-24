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
            <Link to={`/methodologies/${slug}`}>{title}</Link>{' '}
          </>
        ))
      ) : (
        <></>
      )}
    </ul>
  );
}

export default MethodologyList;
