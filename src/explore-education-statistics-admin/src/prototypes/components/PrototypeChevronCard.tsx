import classNames from 'classnames';
import React from 'react';
import styles from './PrototypeChevronCard.module.scss';

interface Props {
  title?: string;
  description?: string;
}

const ChevronCard = ({ title, description }: Props) => {
  return (
    <li className={styles.card}>
      <div className={styles.wrapper}>
        <h4 className="govuk-heading-m govuk-!-margin-bottom-0">
          <a
            href="#"
            className={classNames(styles.cardLink, 'govuk-!-margin-bottom-2')}
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
