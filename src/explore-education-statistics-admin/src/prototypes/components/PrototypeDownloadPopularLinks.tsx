import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import React from 'react';

const PrototypeDownloadPopularLinks = () => {
  const [showDownloadModal, toggleDownloadModal] = useToggle(false);
  return (
    <div className="govuk-grid-row">
      <div className="govuk-grid-column-three-quarters">
        <h4 className="govuk-heading-s">Download individual files</h4>
        <ul className="govuk-list govuk-list--bullet govuk-list--spaced">
          <li>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={() => {
                toggleDownloadModal(true);
              }}
            >
              A1 - Children looked after at 31 March by gender, age at 31 March,
              category of need, ethnic origin, legal status and motherhood
              status, in England, 2018 to 2020
            </a>
          </li>
          <li>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={() => {
                toggleDownloadModal(true);
              }}
            >
              A2 - Children looked after at 31 March by placement, in England,
              2018 to 2020
            </a>
          </li>
          <li>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={() => {
                toggleDownloadModal(true);
              }}
            >
              A3 - Unaccompanied asylum-seeking children looked after at 31
              March by gender, age at 31 March, category of need and ethnic
              origin, in England, 2018 to 2020
            </a>
          </li>
          <li>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={() => {
                toggleDownloadModal(true);
              }}
            >
              A4 - Children looked after at 31 March by distance between home
              and placement and locality of placement, in England, 2018 to 2020
            </a>
          </li>
          <li>
            <a
              href="#"
              className="govuk-link govuk-link--no-visited-state"
              onClick={() => {
                toggleDownloadModal(true);
              }}
            >
              A5 - Children looked after at 31 March by placement, placement
              location and placement provider, in England, 2018 to 2020
            </a>
          </li>
        </ul>
        <ModalConfirm
          open={showDownloadModal}
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
      </div>
    </div>
  );
};

export default PrototypeDownloadPopularLinks;
