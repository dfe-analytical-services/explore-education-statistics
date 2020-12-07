import Details from '@common/components/Details';
import {
  PublicationDownloadSummary,
  Topic,
} from '@common/services/themeService';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  topic: Topic<PublicationDownloadSummary>;
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

function TopicDownloadList({ topic }: Props) {
  return (
    <Details summary={topic.title}>
      {topic.publications.map(({ id, title, downloadFiles }) => (
        <React.Fragment key={id}>
          <h3 className="govuk-heading-s">{title}</h3>

          <ul>
            {downloadFiles.map(({ extension, name, path, size }) => (
              <li key={path}>
                <Link
                  to={`${process.env.DATA_API_BASE_URL}/download/${path}`}
                  data-testid={`download-stats-${path}`}
                  analytics={{
                    category: 'Downloads',
                    action: `Download latest data page ${name} file downloaded`,
                    label: `File URL: /api/download/${path}`,
                  }}
                >
                  {`${name}${getPublicationDate(path)}`}
                </Link>
                {` (${extension}, ${size})`}
              </li>
            ))}
          </ul>
        </React.Fragment>
      ))}
    </Details>
  );
}

export default TopicDownloadList;
