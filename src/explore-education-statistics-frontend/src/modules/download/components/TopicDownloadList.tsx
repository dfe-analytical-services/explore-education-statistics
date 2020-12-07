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

function TopicDownloadList({ topic }: Props) {
  return (
    <Details summary={topic.title}>
      {topic.publications.map(
        ({
          id,
          title,
          downloadFiles,
          earliestReleaseTime,
          latestReleaseTime,
        }) => (
          <React.Fragment key={id}>
            <h3 className="govuk-heading-s govuk-!-margin-bottom-2">{title}</h3>

            <p>
              {earliestReleaseTime === latestReleaseTime
                ? earliestReleaseTime
                : `${earliestReleaseTime} to ${latestReleaseTime}`}
            </p>

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
                    {name}
                  </Link>
                  {` (${extension}, ${size})`}
                </li>
              ))}
            </ul>
          </React.Fragment>
        ),
      )}
    </Details>
  );
}

export default TopicDownloadList;
