import ContentHtml from '@common/components/ContentHtml';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { PublicationListSummary } from '@common/services/publicationService';
import { releaseTypes } from '@common/services/types/releaseType';
import styles from '@frontend/modules/find-statistics/components/PublicationSummary.module.scss';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  publication: PublicationListSummary;
}

const PublicationSummary = ({ publication }: Props) => {
  const {
    published,
    slug,
    summary,
    highlightContent,
    highlightSummary,
    highlightTitle,
    theme,
    title,
    type,
    latestReleaseSlug,
  } = publication;
  return (
    <li className={`${styles.container} govuk-!-margin-top-4`}>
      <h3 className="govuk-!-margin-bottom-2">
        <Link to={`/find-statistics/${slug}/${latestReleaseSlug}`}>
          {highlightTitle ? (
            <ContentHtml
              html={highlightTitle}
              sanitizeOptions={{ allowedTags: ['em'] }}
              wrapperElement="span"
            />
          ) : (
            title
          )}
        </Link>
      </h3>

      {highlightSummary ? (
        <ContentHtml
          html={highlightSummary}
          sanitizeOptions={{ allowedTags: ['em'] }}
          wrapperElement="p"
        />
      ) : (
        <p>{summary}</p>
      )}

      {highlightContent && (
        <ContentHtml
          html={highlightContent}
          sanitizeOptions={{ allowedTags: ['em'] }}
          className={styles.highlightContent}
          testId="search-highlight"
          wrapperElement="p"
        />
      )}

      <SummaryList
        className="govuk-!-margin-bottom-4 govuk-!-margin-top-4"
        compact
        noBorder
      >
        <SummaryListItem term="Release type">
          {releaseTypes[type]}
        </SummaryListItem>
        <SummaryListItem term="Published">
          <FormattedDate format="d MMM yyyy">{published}</FormattedDate>
        </SummaryListItem>
        <SummaryListItem term="Theme">{theme}</SummaryListItem>
      </SummaryList>
    </li>
  );
};
export default PublicationSummary;
