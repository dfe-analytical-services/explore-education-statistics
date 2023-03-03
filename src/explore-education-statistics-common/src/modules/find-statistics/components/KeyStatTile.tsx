import React, { ReactNode } from 'react';
import classNames from 'classnames';
import formatPretty from '@common/utils/number/formatPretty';
import styles from './KeyStatTile.module.scss';

interface Props {
  children?: ReactNode;
  isReordering?: boolean;
  testId?: string;
  title: string;
  titleTag?: 'h3' | 'h4';
  value: string;
}

const KeyStatTile = ({
  children,
  isReordering = false,
  testId = 'keyStatTile',
  titleTag: TitleElement = 'h3',
  title,
  value,
}: Props) => {
  return (
    <div
      className={classNames(styles.tile, {
        [styles.reordering]: isReordering,
      })}
    >
      <TitleElement className="govuk-heading-s" data-testid={`${testId}-title`}>
        {title}
      </TitleElement>

      <p className="govuk-heading-xl" data-testid={`${testId}-statistic`}>
        {formatPretty(value)}
      </p>

      {children}
    </div>
  );
};

export default KeyStatTile;
