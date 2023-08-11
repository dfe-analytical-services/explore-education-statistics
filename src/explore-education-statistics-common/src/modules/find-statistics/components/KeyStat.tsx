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

interface KeyStatWrapperProps {
  children: ReactNode;
  testId?: string;
}

export const KeyStatWrapper = ({ children, testId }: KeyStatWrapperProps) => {
  return (
    <div className={styles.wrapper} data-testid={testId}>
      {children}
    </div>
  );
};

export interface KeyStatProps {
  children?: ReactNode;
  guidanceTitle?: string;
  guidanceText?: string;
  includeWrapper?: boolean;
  statistic: string;
  testId?: string;
  title: string;
  trend?: string;
}

const KeyStat = ({
  children,
  guidanceTitle = 'Help',
  guidanceText,
  includeWrapper = true,
  statistic,
  testId = 'keyStat',
  title,
  trend,
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

  return includeWrapper ? (
    <KeyStatWrapper testId={testId}>{body}</KeyStatWrapper>
  ) : (
    body
  );
};

export default KeyStat;
