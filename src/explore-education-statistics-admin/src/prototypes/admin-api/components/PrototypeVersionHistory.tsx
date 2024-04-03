import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import classNames from 'classnames';
import React, { useState } from 'react';
import capitalize from 'lodash/capitalize';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import FormattedDate from '@common/components/FormattedDate';
import { formatPartialDate } from '@common/utils/date/partialDate';
import PrototypeChangeStatusForm, {
  StatusFormValues,
} from './PrototypeChangeStatusForm';

const PrototypeVersionHistory = () => {
  const [statusModal, toggleStatusModal] = useToggle(false);
  const [statusNotesModal, toggleStatusNotesModal] = useToggle(false);
  const [changelogModal, toggleChangelogModal] = useToggle(false);
  const [selectedVersion, setSelectedVersion] = useState('');

  const [version1Status, setVersion1Status] = useState<StatusFormValues>({
    status: 'live',
  });
  const [version2Status, setVersion2Status] = useState<StatusFormValues>({
    status: 'live',
  });
  const [version3Status, setVersion3Status] = useState<StatusFormValues>({
    status: 'live',
  });

  function getStatus() {
    if (selectedVersion === '2.1') {
      return version1Status;
    }
    if (selectedVersion === '2.0') {
      return version2Status;
    }
    return version3Status;
  }

  return (
    <>
      <table className="govuk-!-margin-top-4 govuk-!-margin-bottom-6">
        <caption className="govuk-!-margin-bottom-3 govuk-table__caption govuk-table__caption--m">
          Version history **Example set in the future**
        </caption>
        <thead>
          <tr>
            <th>Version</th>
            <th>Related release</th>
            <th>Status</th>
            <th className="govuk-table__header--numeric" colSpan={2}>
              Actions
            </th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>2.1</td>
            <td>Academic year 2023/24</td>
            <td>
              <div
                className={classNames(
                  'govuk-tag',
                  version1Status.status === 'deprecated'
                    ? 'govuk-tag--red'
                    : '',
                )}
              >
                {capitalize(version1Status.status)}
              </div>{' '}
            </td>
            <td className="govuk-table__header--numeric">
              {version1Status.status === 'deprecated' && (
                <>
                  {' '}
                  <a
                    href="#"
                    onClick={e => {
                      e.preventDefault();
                      setSelectedVersion('2.1');
                      toggleStatusNotesModal.on();
                    }}
                  >
                    View notes
                  </a>
                </>
              )}
              {version1Status.status !== 'deprecated' && (
                <ButtonText onClick={toggleChangelogModal.on}>
                  View changelog
                </ButtonText>
              )}
            </td>
            <td className="govuk-table__header--numeric">
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setSelectedVersion('2.1');
                  toggleStatusModal.on();
                }}
              >
                Edit status
              </a>
            </td>
          </tr>
          <tr>
            <td>2.0</td>
            <td>Academic year 2022/23</td>
            <td>
              <div
                className={classNames(
                  'govuk-tag',
                  version2Status.status === 'deprecated'
                    ? 'govuk-tag--red'
                    : '',
                )}
              >
                {version2Status.status}
              </div>{' '}
            </td>
            <td className="govuk-table__header--numeric">
              {version2Status.status === 'deprecated' && (
                <>
                  {' '}
                  <a
                    href="#"
                    onClick={e => {
                      e.preventDefault();
                      setSelectedVersion('2.0');
                      toggleStatusNotesModal.on();
                    }}
                  >
                    View notes
                  </a>
                </>
              )}
              {version2Status.status !== 'deprecated' && (
                <ButtonText onClick={toggleChangelogModal.on}>
                  View changelog
                </ButtonText>
              )}
            </td>
            <td className="govuk-table__header--numeric">
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setSelectedVersion('2.0');
                  toggleStatusModal.on();
                }}
              >
                Edit status
              </a>
            </td>
          </tr>
          <tr>
            <td>1.0</td>
            <td>Academic year 2021/22</td>
            <td>
              <div
                className={classNames(
                  'govuk-tag',
                  version3Status.status === 'deprecated'
                    ? 'govuk-tag--red'
                    : '',
                )}
              >
                {version3Status.status}
              </div>{' '}
            </td>
            <td className="govuk-table__header--numeric">
              {version3Status.status === 'deprecated' && (
                <>
                  {' '}
                  <a
                    href="#"
                    onClick={e => {
                      e.preventDefault();
                      setSelectedVersion('1.0');
                      toggleStatusNotesModal.on();
                    }}
                  >
                    View notes
                  </a>
                </>
              )}
              {version3Status.status !== 'deprecated' && (
                <ButtonText onClick={toggleChangelogModal.on}>
                  View changelog
                </ButtonText>
              )}
            </td>
            <td className="govuk-table__header--numeric">
              <a
                href="#"
                onClick={e => {
                  e.preventDefault();
                  setSelectedVersion('1.0');
                  toggleStatusModal.on();
                }}
              >
                Edit status
              </a>
            </td>
          </tr>
        </tbody>
      </table>
      <Modal
        open={statusModal}
        title={`API data set version ${selectedVersion}`}
        className="govuk-!-width-one-half"
      >
        <PrototypeChangeStatusForm
          selectedStatus={getStatus()}
          onSubmit={values => {
            if (selectedVersion === '2.1') {
              setVersion1Status(values);
            } else if (selectedVersion === '2.0') {
              setVersion2Status(values);
            } else {
              setVersion3Status(values);
            }
            toggleStatusModal.off();
          }}
        />
      </Modal>

      <Modal
        open={statusNotesModal}
        title={`Deprecated API data set, version ${selectedVersion}`}
        className="govuk-!-width-one-half"
      >
        <h3 className="govuk-heading-s">Notes</h3>
        <div
          className="govuk-!-margin-bottom-9"
          style={{ whiteSpace: 'pre-wrap' }}
        >
          {selectedVersion === '2.1' && (
            <>
              <p>{version1Status.notes}</p>
              <h3 className="govuk-heading-s">Expiry date for this data set</h3>

              {version1Status.date && (
                <p>
                  {version1Status.date.day ? (
                    <FormattedDate format="d MMM yyyy">
                      {
                        new Date(
                          `${version1Status.date.month}/${version1Status.date.day}/${version1Status.date.year}`,
                        )
                      }
                    </FormattedDate>
                  ) : (
                    <time>{formatPartialDate(version1Status.date)}</time>
                  )}
                </p>
              )}
            </>
          )}
          {selectedVersion === '2.0' && (
            <>
              <p>{version2Status.notes}</p>
              <h3 className="govuk-heading-s">Deprecation date</h3>

              {version2Status.date && (
                <p>
                  {version2Status.date.day ? (
                    <FormattedDate format="d MMM yyyy">
                      {
                        new Date(
                          `${version2Status.date.month}/${version2Status.date.day}/${version2Status.date.year}`,
                        )
                      }
                    </FormattedDate>
                  ) : (
                    <time>{formatPartialDate(version2Status.date)}</time>
                  )}
                </p>
              )}
            </>
          )}
          {selectedVersion === '1.0' && (
            <>
              <p>{version3Status.notes}</p>
              <h3 className="govuk-heading-s">Deprecation date</h3>

              {version3Status.date && (
                <p>
                  {version3Status.date.day ? (
                    <FormattedDate format="d MMM yyyy">
                      {
                        new Date(
                          `${version3Status.date.month}/${version3Status.date.day}/${version3Status.date.year}`,
                        )
                      }
                    </FormattedDate>
                  ) : (
                    <time>{formatPartialDate(version3Status.date)}</time>
                  )}
                </p>
              )}
            </>
          )}
        </div>
        <Button
          onClick={() => {
            toggleStatusNotesModal(false);
          }}
        >
          Close
        </Button>
      </Modal>

      <Modal
        open={changelogModal}
        title="Changelog"
        className="govuk-!-width-one-half"
        onExit={toggleChangelogModal.off}
      >
        <>
          <h3>Version notes</h3>
          <p>
            This is a minor update on the previous version, some new locations,
            filters and indicators have been added to the data set since the
            previous release, please see the details in the changelog below.
          </p>

          <Button onClick={toggleChangelogModal.off}>Close</Button>
        </>
      </Modal>
    </>
  );
};
export default PrototypeVersionHistory;
