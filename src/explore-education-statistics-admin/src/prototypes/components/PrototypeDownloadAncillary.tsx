import Details from '@common/components/Details';
import React from 'react';
import styles from '../PrototypePublicPage.module.scss';
import PrototypeDownloadAncillaryLinks from './PrototypeDownloadAncillaryLinks';

interface Props {
  viewAsList?: boolean;
}

const PrototypeDownloadAncillary = ({ viewAsList }: Props) => {
  return (
    <>
      <div className={styles.prototypeDownloadContainer}>
        <h3 className="govuk-heading-m">Ancillary files</h3>
        <p>
          <a
            href="#"
            className="govuk-button govuk-button--secondary  govuk-!-margin-bottom-0"
          >
            <span className="govuk-visually-hidden">ancillary files </span> ZIP
            (1Mb)
          </a>
        </p>
      </div>
      <div className="govuk-!-margin-bottom-9">
        {viewAsList && <PrototypeDownloadAncillaryLinks />}
        {!viewAsList && (
          <Details
            summary="Select specific files"
            className="govuk-!-margin-bottom-9"
          >
            <PrototypeDownloadAncillaryLinks />
          </Details>
        )}
      </div>
    </>
  );
};

export default PrototypeDownloadAncillary;
