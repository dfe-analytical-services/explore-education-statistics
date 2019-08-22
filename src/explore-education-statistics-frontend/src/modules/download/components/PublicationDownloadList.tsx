import { baseUrl } from '@common/services/api';
import Link from '@frontend/components/Link';
import React from 'react';

export interface Publication {
  id: string;
  slug: string;
  summary: string;
  title: string;
  downloadFiles: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
}

interface Props {
  publications: Publication[];
}

function getPublicationDate(path: string) {
  const twoYearDateFormat = /.*\/([0-9]{4})-([0-9]{2})\/.*/;
  const singleYearDateFormat = /.*\/([0-9]{4})\/.*/;

  const twoYearDate = twoYearDateFormat.exec(path);
  const singleYearDate = singleYearDateFormat.exec(path);

  if (twoYearDate) {
    return `, ${twoYearDate[1]}/${twoYearDate[2]}`;
  }

  if (singleYearDate) {
    return `, ${singleYearDate[1]}`;
  }
  return '';
}

function PublicationList({ publications }: Props) {
  return (
    <>
      {publications.length > 0 ? (
        publications.map(({ id, title, downloadFiles }) => (
          <React.Fragment key={id}>
            <h3 className="govuk-heading-s">Download files for: {title}</h3>
            <ul className="govuk-list govuk-list--bullet">
              {downloadFiles.map(({ extension, name, path, size }) => (
                <li key={path}>
                  <Link
                    to={`${baseUrl.data}/api/download/${path}`}
                    className="govuk-link"
                    data-testid={`download-stats-${path}`}
                  >
                    {`${name}${getPublicationDate(path)}`}
                  </Link>
                  {` (${extension}, ${size})`}
                </li>
              ))}
            </ul>
          </React.Fragment>
        ))
      ) : (
        <></>
      )}
    </>
  );
}

export default PublicationList;
