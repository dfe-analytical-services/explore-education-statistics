import Details from '@common/components/Details';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React, { ReactNode } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';

interface KeyStatContainerProps {
  children: ReactNode;
}

export const KeyStatContainer = ({ children }: KeyStatContainerProps) => {
  return <div className={styles.container}>{children}</div>;
};

interface KeyStatColumnProps {
  children: ReactNode;
  testId?: string;
}

export const KeyStatColumn = ({ children, testId }: KeyStatColumnProps) => {
  return (
    <div className={styles.column} data-testid={testId}>
      {children}
    </div>
  );
};

export interface KeyStatProps {
  children?: ReactNode;
  title: string;
  statistic: string;
  trend?: string;
  guidanceTitle?: string;
  guidanceText?: string;
  hasColumn?: boolean;
  testId?: string;
}

const KeyStat = ({
  children,
  title,
  statistic,
  trend,
  guidanceTitle = 'Help',
  guidanceText,
  hasColumn = true,
  testId = 'keyStat',
}: KeyStatProps) => {
  const body = (
    <>
      <KeyStatTile title={title} value={statistic} testId={testId}>
        {trend && (
          <p className="govuk-body-s" data-testid={`${testId}-trend`}>
            {trend}
          </p>
        )}
      </KeyStatTile>

      {guidanceText && (
        <Details
          summary={guidanceTitle}
          className={styles.guidanceTitle}
          hiddenText={guidanceTitle === 'Help' ? `for ${title}` : undefined}
        >
          <div data-testid={`${testId}-guidanceText`}>
            <ReactMarkdown key={guidanceText}>{guidanceText}</ReactMarkdown>
          </div>
        </Details>
      )}

      {children}
    </>
  );

  return hasColumn ? (
    <KeyStatColumn testId={testId}>{body}</KeyStatColumn>
  ) : (
    body
  );
};

export default KeyStat;
