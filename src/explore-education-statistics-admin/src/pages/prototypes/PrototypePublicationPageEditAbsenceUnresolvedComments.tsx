/* eslint-disable no-shadow */
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import { ExtendedComment } from '@admin/services/publicationService';
import React from 'react';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

interface Props {
  newBlankRelease?: boolean;
  reviewing?: boolean;
}

const PublicationPage = ({ reviewing, newBlankRelease }: Props) => {
  const [editing, setEditing] = React.useState(newBlankRelease);

  const [data] = React.useState(
    PrototypePublicationService.getNewPublication(),
  );

  const [allUnresolved, setAllUnresolved] = React.useState<ExtendedComment[]>(
    [],
  );

  React.useEffect(() => {
    const allComments = [
      ...data.keyStatistics.comments,
      ...data.content.reduce<ExtendedComment[]>((allComments, content) => {
        return [
          ...allComments,
          ...content.content.reduce<ExtendedComment[]>(
            (all, _) => [...all, ..._.comments],
            [],
          ),
        ];
      }, []),
    ];

    setAllUnresolved(allComments.filter(c => c.state === 'open'));
  }, [data]);

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=readyApproval',
          text: 'Administrator dashboard',
        },
        { text: 'Edit pupil absence statistics', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="addContent" task="editRelease" />

      <div className="govuk-form-group">
        {allUnresolved.length > 0 && (
          <div className="govuk-warning-text">
            <span className="govuk-warning-text__icon" aria-hidden="true">
              !
            </span>
            <strong className="govuk-warning-text__text">
              <span className="govuk-warning-text__assistive">Warning</span>
              There are {allUnresolved.length} unresolved comments
            </strong>
          </div>
        )}

        <fieldset className="govuk-fieldset dfe-toggle-edit">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            <h1 className="govuk-fieldset__heading">Set page view</h1>
          </legend>
          <div className="govuk-radios govuk-radios--inline">
            <div className="govuk-radios__item">
              {newBlankRelease && (
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  defaultChecked
                  onChange={() => {
                    setEditing(true);
                  }}
                />
              )}
              {reviewing && (
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  onChange={() => {
                    setEditing(true);
                  }}
                />
              )}
              <label className="govuk-label govuk-radios__label" htmlFor="edit">
                Add / view comments and edit content
              </label>
            </div>
            <div className="govuk-radios__item">
              {newBlankRelease && (
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  onChange={() => {
                    setEditing(false);
                  }}
                />
              )}
              {reviewing && (
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  defaultChecked
                  onChange={() => {
                    setEditing(false);
                  }}
                />
              )}

              <label className="govuk-label govuk-radios__label" htmlFor="edit">
                Preview content
              </label>
            </div>
          </div>
        </fieldset>
      </div>

      <hr />
      <div
        className={`govuk-width-container ${
          editing ? 'dfe-align--comments' : 'dfe-hide-comments'
        }`}
      >
        <EditablePublicationPage editing={editing} reviewing data={data} />
      </div>
    </PrototypePage>
  );
};

export default PublicationPage;
