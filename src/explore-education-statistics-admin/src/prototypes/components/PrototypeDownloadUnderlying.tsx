import Details from '@common/components/Details';
import React from 'react';
import styles from '../PrototypePublicPage.module.scss';
import PrototypeDownloadUnderlyingLinks from './PrototypeDownloadUnderlyingLinks';

interface Props {
  viewAsList?: boolean;
}

const PrototypeDownloadUnderlying = ({ viewAsList }: Props) => {
  return (
    <>
      <div className={styles.prototypeDownloadContainer}>
        <h3 className="govuk-heading-m">Underlying data</h3>
        <p>
          <a
            href="#"
            className="govuk-button govuk-button--secondary  govuk-!-margin-bottom-0 govuk-!-margin-right-3"
          >
            <span className="govuk-visually-hidden">Underlying data </span> CSV
            (2Mb)
          </a>
          <a
            href="#"
            className="govuk-button govuk-button--secondary  govuk-!-margin-bottom-0"
          >
            <span className="govuk-visually-hidden">Underlying data </span> ODS
            (2Mb)
          </a>
        </p>
      </div>
      <div className="govuk-!-margin-bottom-9">
        {viewAsList && <PrototypeDownloadUnderlyingLinks />}
        {!viewAsList && (
          <Details
            summary="Select specific files"
            className="govuk-!-margin-bottom-9"
          >
            <PrototypeDownloadUnderlyingLinks />
          </Details>
        )}
      </div>
    </>
  );
};

export default PrototypeDownloadUnderlying;
