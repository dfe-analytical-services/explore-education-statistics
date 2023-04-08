import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import Button from '@common/components/Button';
import { FormGroup, FormTextInput } from '@common/components/form';
import FormEditor from '@admin/components/form/FormEditor';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';

const PrototypeBauGlossary = () => {
  const [edit, setEdit] = useState(false);
  const [newEntry, setNewEntry] = useState(false);
  const [editItem, setEditItem] = useState('');
  const [showDeleteModal, toggleDeleteModal] = useToggle(false);

  const glossaryList = [
    {
      name: 'Absence',
      description: `<p>When a pupil misses (or is absent from) at least 1 possible school session.</p><p>Counted in sessions, where 1 session is equivalent to half-a-day.</p><p>There are 4 types of absence:</p><ul><li><a href="/glossary#authorised-absence">authorised absence</a></li><li><a href="/glossary#overall-absence">overall absence</a></li><li><a href="/glossary#persistent-absence">persistent absence</a></li><li><a href="/glossary#unauthorised-absence">unauthorised absence</a></li></ul>`,
      log: '-',
    },
    {
      name: 'Academic year',
      description: `<p>Lasts from 31 August to 31 July. Generally broken into 3 terms - autumn, spring and summer.</p>`,
      log: '-',
    },
    {
      name: 'Ad hoc statistics',
      description: `<p>Releases of statistics which are not part of DfE's regular annual official statistical release calendar.</p>`,
      log: '14 July 2020, 16:52',
    },
    {
      name: 'Authorised absence',
      description: `<p>Releases of statistics which are not part of DfE's regular annual official statistical release calendar.</p>`,
      log: '-',
    },
    {
      name: 'Dual main registered pupils',
      description: `<p>Dual registered pupils who are enrolled at more than 1 school have a dual main registration (at their main school) and 1 or more subsidiary registrations (at their additional schools).</p><p>See also <a href="/glossary#dual-registered-pupils">Dual registered pupils</a>.</p>`,
      log: '-',
    },
    {
      name: 'Dual registered pupils',
      description: `<p>Pupils who are enrolled at more than 1 school.</p><p>See also <a href="/glossary#dual-main-registered-pupils">dual main registered pupils</a>.</p>`,
      log: '10 July 2020 12:34',
    },
    {
      name: 'Exclusion',
      description: `<p>When a pupil is not allowed to attend (or is excluded from) a school.</p><p>There are 2 types of exclusion:</p><ul><li><a href="/glossary#fixed-period-exclusion">fixed-period exclusion</a></li><li><a href="/glossary#permanent-exclusion">permanent exclusion</a></li></ul>`,
      log: '-',
    },
  ];

  glossaryList.sort((a, b) => (a.name > b.name ? 1 : -1));

  const findItem = glossaryList.find(item => item.name === editItem);

  let glossaryName = findItem?.name;
  const glossaryDescription = findItem?.description;

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '#' },
        { name: 'Manage glossary', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle title="Manage glossary" caption="Content and data" />
        </div>
      </div>

      {edit && (
        <>
          <form id="createMetaForm" className="govuk-!-marin-bottom-9">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
              <h2 className="govuk-heading-m">
                {newEntry ? 'Add new glossary item' : 'Update glossary item'}
              </h2>
            </legend>
            <FormGroup>
              <FormTextInput
                id="name"
                name="name"
                label="Glossary item"
                value={glossaryName}
                className="govuk-!-width-one-half"
                onChange={event => {
                  glossaryName = event.target.value;
                }}
              />
            </FormGroup>
            <FormEditor
              id="description"
              label="Item description"
              value={glossaryDescription || ''}
              onChange={() => {
                setEdit(true);
              }}
            />
            <div className="govuk-!-margin-top-9 govuk-grid-row">
              <div className="govuk-grid-column-one-half">
                <Button
                  className="govuk-!-margin-right-3"
                  onClick={() => {
                    setEdit(false);
                  }}
                >
                  Save
                </Button>

                <Button
                  variant="secondary"
                  onClick={() => {
                    setEdit(false);
                  }}
                >
                  Cancel
                </Button>
              </div>
              <div className="govuk-grid-column-one-half dfe-align--right">
                {!newEntry && (
                  <>
                    <Button
                      variant="warning"
                      onClick={() => {
                        toggleDeleteModal(true);
                      }}
                    >
                      Delete this item
                    </Button>
                    <ModalConfirm
                      open={showDeleteModal}
                      title="Confirm delete"
                      onExit={() => toggleDeleteModal(false)}
                      onConfirm={() => toggleDeleteModal(false)}
                      onCancel={() => toggleDeleteModal(false)}
                    >
                      <p>Are you sure you want to delete this item?</p>
                    </ModalConfirm>
                  </>
                )}
              </div>
            </div>
          </form>
        </>
      )}
      {!edit && (
        <>
          <Button
            className="govuk-!-margin-right-3"
            onClick={() => {
              setEdit(true);
              setNewEntry(true);
              setEditItem('');
            }}
          >
            Add new item
          </Button>
          <table>
            <thead>
              <tr>
                <th scope="col">Glossary item</th>
                <th scope="col">Last updated</th>
              </tr>
            </thead>
            <tbody>
              {glossaryList.map((item, index) => (
                // eslint-disable-next-line react/no-array-index-key
                <tr key={index.toString()}>
                  <td>
                    <a
                      href="#"
                      onClick={() => {
                        setEditItem(item.name);
                        setEdit(true);
                        setNewEntry(false);
                      }}
                    >
                      {item.name}
                    </a>
                  </td>
                  <td>{item.log}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </PrototypePage>
  );
};

export default PrototypeBauGlossary;
