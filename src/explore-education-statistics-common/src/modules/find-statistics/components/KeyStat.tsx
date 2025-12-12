import Details from '@common/components/Details';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import React, { ReactNode } from 'react';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import classNames from 'classnames';

interface KeyStatContainerProps {
  children: ReactNode;
}

export const KeyStatContainer = ({ children }: KeyStatContainerProps) => {
  return <div className={styles.container}>{children}</div>;
};

interface KeyStatWrapperProps {
  children: ReactNode;
  isRedesignStyle?: boolean;
  testId?: string;
}

export const KeyStatWrapper = ({
  children,
  isRedesignStyle = false,
  testId,
}: KeyStatWrapperProps) => {
  return (
    <div
      className={classNames(styles.wrapper, {
        [styles.wrapperRedesign]: isRedesignStyle,
      })}
      data-testid={testId}
    >
      {children}
    </div>
  );
};

export interface KeyStatProps {
  children?: ReactNode;
  guidanceTitle?: string;
  guidanceText?: string;
  includeWrapper?: boolean;
  isRedesignStyle?: boolean;
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
  isRedesignStyle = false,
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
          className={
            isRedesignStyle
              ? styles.guidanceTitleRedesign
              : styles.guidanceTitle
          }
          showCloseButton={isRedesignStyle}
          hiddenText={guidanceTitle === 'Help' ? `for ${title}` : undefined}
        >
          <div data-testid={`${testId}-guidanceText`}>
            <p className={styles.guidanceText}>{guidanceText}</p>
          </div>
        </Details>
      )}

      {children}
    </>
  );

  return includeWrapper ? (
    <KeyStatWrapper testId={testId} isRedesignStyle={isRedesignStyle}>
      {body}
    </KeyStatWrapper>
  ) : (
    body
  );
};

export default KeyStat;
