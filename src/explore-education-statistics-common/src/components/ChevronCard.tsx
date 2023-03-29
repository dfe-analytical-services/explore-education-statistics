import styles from '@common/components/ChevronCard.module.scss';
import classNames from 'classnames';
import React, { cloneElement, ReactElement, ReactNode } from 'react';

interface Props {
  description: string;
  descriptionAfter?: ReactNode;
  link?: ReactNode;
  showChevron?: boolean;
}

export default function ChevronCard({
  description,
  descriptionAfter,
  link,
  showChevron = true,
}: Props) {
  return (
    <li className={styles.card}>
      <div className={styles.wrapper}>
        <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
          {link &&
            cloneElement(link as ReactElement, {
              className: classNames(styles.link, {
                [styles.linkWithChevron]: showChevron,
              }),
            })}
        </h3>
        <p className={styles.description}>{description}</p>
        {descriptionAfter && descriptionAfter}
      </div>
    </li>
  );
}
