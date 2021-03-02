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
          latestReleaseId,
        }) => (
          <React.Fragment key={id}>
            <h3 className="govuk-heading-s govuk-!-margin-bottom-2">{title}</h3>

            <p>
              {earliestReleaseTime === latestReleaseTime
                ? earliestReleaseTime
                : `${earliestReleaseTime} to ${latestReleaseTime}`}
            </p>

            <ul data-testid={title}>
              {downloadFiles.map(
                ({ id: fileId, fileName, extension, name, size }) => {
                  const isAllFiles = !fileId && name === 'All files';

                  const url = `${
                    process.env.CONTENT_API_BASE_URL
                  }/releases/${latestReleaseId}/files/${
                    isAllFiles ? 'all' : fileId
                  }`;

                  return (
                    <li key={isAllFiles ? 'all' : fileId}>
                      <Link
                        to={url}
                        analytics={{
                          category: 'Downloads',
                          action: `Download latest data page ${
                            isAllFiles ? 'all files' : 'file'
                          } downloaded`,
                          label: `Publication: ${title}, File: ${fileName}`,
                        }}
                      >
                        {name}
                      </Link>
                      {` (${extension}, ${size})`}
                    </li>
                  );
                },
              )}
            </ul>
          </React.Fragment>
        ),
      )}
    </Details>
  );
}

export default TopicDownloadList;
