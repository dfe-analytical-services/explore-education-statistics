import { baseUrl } from '@common/services/api';
import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
  slug: string;
  summary: string;
  title: string;
  dataFiles: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
}

interface Props {
  publications: Publication[];
}

function PublicationList({ publications }: Props) {
  return (
    <>
      {publications.length > 0 ? (
        publications.map(({ id, title, dataFiles }) => (
          <div className="govuk-!-margin-bottom-9" key={id}>
            <h3 className="govuk-heading-s">Download files for: {title}</h3>
            {dataFiles.map(({ extension, name, path, size }) => (
              <p key={path}>
                <Link
                  to={`${baseUrl.data}/api/download/${path}`}
                  className="govuk-link"
                  data-testid={`download-stats-${path}`}
                >
                  {name}
                </Link>
                {` `}({extension}, {size})
              </p>
            ))}
          </div>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default PublicationList;
