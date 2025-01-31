import Details from '@common/components/Details';
import React from 'react';
import styles from '../PrototypePublicPage.module.scss';
import PrototypeDownloadPopularLinks from './PrototypeDownloadPopularLinks';

interface Props {
  viewAsList?: boolean;
}

const PrototypeDownloadPopular = ({ viewAsList }: Props) => {
  return (
    <>
      <div className={styles.prototypeDownloadContainer}>
        <h3 className="govuk-heading-m">Featured tables</h3>
        <p>
          <a
            href="#"
            className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-3"
          >
            <span className="govuk-visually-hidden">Featured tables </span> XLS
            (1Mb)
          </a>
          <a
            href="#"
            className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
          >
            <span className="govuk-visually-hidden">Featured tables </span> ODS
            (1Mb)
          </a>
        </p>
      </div>
      <div className="govuk-!-margin-bottom-9">
        {viewAsList && <PrototypeDownloadPopularLinks />}
        {!viewAsList && (
          <Details
            summary="Select specific files"
            className="govuk-!-margin-bottom-9"
          >
            <PrototypeDownloadPopularLinks />
          </Details>
        )}
      </div>
    </>
  );
};

export default PrototypeDownloadPopular;
