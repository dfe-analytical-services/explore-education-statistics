import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import { FormGroup, FormTextInput } from '@common/components/form';
import FormEditor from '@admin/components/form/FormEditor';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';

const PrototypeExamplePage = () => {
  const [edit, setEdit] = useState(false);
  const [newEntry, setNewEntry] = useState(false);
  const [editItem, setEditItem] = useState('');
  const [editItemName, setEditItemName] = useState('');
  const [editItemDescription, setEditItemDescription] = useState('');
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
  ];

  glossaryList.sort((a, b) => (a.name > b.name ? 1 : -1));

  const findItem = glossaryList.find(item => item.name === editItem);

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        { name: 'Platform adminisration', link: '#' },
        { name: 'Manage glossary', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">Content and data</span>
            Manage glossary
          </h1>
        </div>
      </div>

      {edit && (
        <>
          <form id="createMetaForm" className="govuk-!-marin-bottom-9">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
              <h2 className="govuk-heading-m">Update glossary item</h2>
            </legend>
            <FormGroup>
              <FormTextInput
                id="name"
                name="name"
                label="Glossary item"
                value={findItem?.name ? findItem?.name : ''}
                className="govuk-!-width-one-half"
                onChange={event => {
                  setEditItemName(event.target.value);
                }}
              />
            </FormGroup>
            <FormEditor
              id="description"
              name="description"
              label="Item description"
              value={findItem?.description ? findItem?.description : ''}
              onChange={event => {
                setEdit(true);
              }}
            />
            <div className="govuk-!-margin-top-9 govuk-grid-row">
              <div className="govuk-grid-column-one-half">
                <button
                  className="govuk-button govuk-!-margin-right-3"
                  type="submit"
                  onClick={() => {
                    setEdit(false);
                  }}
                >
                  Save
                </button>
                <button
                  className="govuk-button govuk-button--secondary"
                  type="submit"
                  onClick={() => {
                    setEdit(false);
                  }}
                >
                  Cancel
                </button>
              </div>
              <div className="govuk-grid-column-one-half dfe-align--right">
                {!newEntry && (
                  <>
                    <button
                      className="govuk-button govuk-button--warning"
                      type="submit"
                      onClick={() => {
                        toggleDeleteModal(true);
                      }}
                    >
                      Delete this item
                    </button>
                    <ModalConfirm
                      mounted={showDeleteModal}
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
          <table className="govuk-table">
            <thead>
              <tr>
                <th scope="col">Glossary item</th>
                <th scope="col">Last updated</th>
              </tr>
            </thead>
            <tbody>
              {glossaryList.map((item, index) => (
                <tr key={index.toString()}>
                  <td>
                    <a
                      href="#"
                      onClick={event => {
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
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => {
              setEdit(true);
              setNewEntry(true);
              setEditItemName('');
              setEditItemDescription('');
            }}
          >
            Add new item
          </button>
        </>
      )}
    </PrototypePage>
  );
};

export default PrototypeExamplePage;
