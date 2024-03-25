import classNames from 'classnames';
import React from 'react';
import styles from './PrototypeChevronCard.module.scss';

interface Props {
  title?: string;
  description?: string;
  url?: string;
  view?: string;
}

const ChevronCard = ({ title, description, url, view }: Props) => {
  return (
    <li className={view === 'list' ? styles.cardListView : styles.card}>
      <div className={styles.wrapper}>
        <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
          <a
            href={url || '#'}
            className={classNames(
              view === 'list' ? styles.list : styles.cardLink,
              'govuk-!-margin-bottom-2',
            )}
          >
            {title}
          </a>
        </h4>
        <p className="govuk-hint">{description}</p>
      </div>
    </li>
  );
};

export default ChevronCard;
