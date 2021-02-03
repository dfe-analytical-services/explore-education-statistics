import classNames from 'classnames';
import React from 'react';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';

const PrototypeDownloadPopular = () => {
  const [showDownloadModal, toggleDownloadModal] = useToggle(false);
  return (
    <>
      <h3 className="govuk-heading-m">Popular tables</h3>
      <p>
        <a
          href="#"
          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-3"
        >
          Download all as xls
        </a>
        <a
          href="#"
          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
        >
          Download all as ods
        </a>
      </p>
      <ul className="govuk-list govuk-list--bullet govuk-list--spaced govuk-!-margin-bottom-6">
        <li>
          <a
            href="#"
            className="govuk-link"
            onClick={() => {
              toggleDownloadModal(true);
            }}
          >
            A1 - Children looked after at 31 March by gender, age at 31 March,
            category of need, ethnic origin, legal status and motherhood status,
            in England, 2018 to 2020
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            A2 - Children looked after at 31 March by placement, in England,
            2018 to 2020
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            A3 - Unaccompanied asylum-seeking children looked after at 31 March
            by gender, age at 31 March, category of need and ethnic origin, in
            England, 2018 to 2020
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            A4 - Children looked after at 31 March by distance between home and
            placement and locality of placement, in England, 2018 to 2020
          </a>
        </li>
        <li>
          <a href="#" className="govuk-link">
            A5 - Children looked after at 31 March by placement, placement
            location and placement provider, in England, 2018 to 2020
          </a>
        </li>
      </ul>
      <ModalConfirm
        mounted={showDownloadModal}
        title="Download file"
        onExit={() => toggleDownloadModal(false)}
        onConfirm={() => toggleDownloadModal(false)}
        onCancel={() => toggleDownloadModal(false)}
      >
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend
              className={classNames(
                'govuk-fieldset__legend',
                'govuk-fieldset__legend--m',
                'govuk-!-margin-bottom-6',
              )}
            >
              Select a format
            </legend>
            <div className="govuk-radios">
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  className="govuk-radios__input"
                  name="subject"
                  id="subject-1"
                />
                <label
                  className={classNames('govuk-label', 'govuk-radios__label')}
                  htmlFor="subject-1"
                >
                  CSV, 2Mb
                </label>
              </div>
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  className="govuk-radios__input"
                  name="subject"
                  id="subject-2"
                />
                <label
                  className={classNames('govuk-label', 'govuk-radios__label')}
                  htmlFor="subject-2"
                >
                  ODB, 2Mb
                </label>
              </div>
            </div>
          </fieldset>
        </div>
      </ModalConfirm>
    </>
  );
};

export default PrototypeDownloadPopular;
