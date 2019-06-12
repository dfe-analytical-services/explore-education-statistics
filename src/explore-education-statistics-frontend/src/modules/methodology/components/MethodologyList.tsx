import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  publications: Publication[];
}

function MethodologyList({ publications }: Props) {
  return (
    <>
      {publications.length > 0 ? (
        publications.map(({ id, title }) => (
          <>
            <ul className="govuk-list govuk-list--bullet">
              <li>
                <Link to="#">Example methodology 1</Link>
              </li>
              <li>
                <Link to="#">Example methodology 2</Link>
              </li>
              <li>
                <Link to="#">Example methodology 3</Link>
              </li>
            </ul>
          </>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default MethodologyList;
