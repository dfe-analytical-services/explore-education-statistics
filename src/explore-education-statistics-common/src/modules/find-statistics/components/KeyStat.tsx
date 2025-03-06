import Details from '@common/components/Details';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React, { ReactNode, RefObject, useCallback, useContext } from 'react';
import ReactMarkdown from 'react-markdown';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { ExportButtonContext } from '@common/components/ExportButtonMenu';

interface KeyStatContainerProps {
  children: ReactNode;
}

export const KeyStatContainer = ({ children }: KeyStatContainerProps) => {
  const test = useContext(ExportButtonContext)

  const measuredRef = useCallback(() : RefObject<HTMLDivElement> | null => {
      console.log(test);
      return test;
  }, [test]);

  return <div ref={measuredRef} className={styles.container}>{children}</div>;
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
    <div>
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
    </div>
  );

  return includeWrapper ? (
    <KeyStatWrapper testId={testId}>{body}</KeyStatWrapper>
  ) : (
    body
  );
};

export default KeyStat;
