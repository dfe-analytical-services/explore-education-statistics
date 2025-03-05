import styles from '@common/components/ChevronCard.module.scss';
import classNames from 'classnames';
import React, { cloneElement, ReactElement, ReactNode } from 'react';

interface Props {
  as?: keyof JSX.IntrinsicElements;
  cardSize?: 'l' | 'm' | 's';
  className?: string;
  description: string;
  descriptionAfter?: ReactNode;
  headingSize?: 'l' | 'm' | 's';
  link?: ReactNode;
  noBorder?: boolean;
  noChevron?: boolean;
}

export default function ChevronCard({
  as: Component = 'li',
  cardSize = 'm',
  className,
  description,
  descriptionAfter,
  headingSize = 's',
  link,
  noBorder = false,
  noChevron = false,
}: Props) {
  return (
    <Component
      className={classNames({
        'govuk-grid-column-one-third-from-desktop': cardSize === 's',
        'govuk-grid-column-one-half-from-desktop': cardSize === 'm',
        'govuk-grid-column-full': cardSize === 'l',
      })}
    >
      <div
        className={classNames(styles.card, className, {
          [styles.noBorder]: noBorder,
        })}
      >
        <h3
          className={classNames('govuk-!-margin-bottom-2', {
            'govuk-heading-s': headingSize === 's',
            'govuk-heading-m': headingSize === 'm',
            'govuk-heading-l': headingSize === 'l',
          })}
        >
          {link &&
            cloneElement(link as ReactElement, {
              className: classNames(styles.link, {
                [styles.linkWithChevron]: !noChevron,
              }),
            })}
        </h3>
        <p className={styles.description}>{description}</p>
        {descriptionAfter && descriptionAfter}
      </div>
    </Component>
  );
}
