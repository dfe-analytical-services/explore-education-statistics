import classNames from 'classnames';
import React, { useState } from 'react';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PrototypeFootnotes from './PrototypeFootnotes';

const PrototypeAddFootnotes = () => {
  const [addNewFootnote, setAddNewFootnote] = useState(false);
  const [addAnotherFootnote, setAddAnotherFootnote] = useState(false);
  const [valueFootnote, setValueFootnote] = useState('');
  const [valueFootnote2, setValueFootnote2] = useState('');
  const [editFootnoteBlock, setEditFootnoteBlock] = useState(false);
  const [editFootnoteBlock2, setEditFootnoteBlock2] = useState(false);

  return (
    <>
      {addNewFootnote && (
        <>
          {!editFootnoteBlock && (
            <>
              <p>Select either one or multiple subject areas from below</p>
              <Accordion id="uploaded-data">
                <AccordionSection heading="Geographical absence">
                  <PrototypeFootnotes />
                </AccordionSection>
                <AccordionSection heading="Local authority">
                  <PrototypeFootnotes />
                </AccordionSection>
                <AccordionSection heading="National characteristics">
                  <PrototypeFootnotes />
                  <button className="govuk-button" type="button">
                    Add footnote
                  </button>
                </AccordionSection>
              </Accordion>
              <h3 className="govuk-heading-s">Footnote</h3>

              <textarea
                className="govuk-textarea govuk-!-margin-bottom-3"
                id="footnote-1"
                name="footnote-1"
                rows={5}
                value={valueFootnote}
                onChange={event => {
                  setValueFootnote(event.target.value);
                }}
              >
                test
              </textarea>
              <button
                className="govuk-button"
                type="submit"
                onClick={() => setEditFootnoteBlock(true)}
              >
                Save
              </button>
            </>
          )}

          {(editFootnoteBlock || editFootnoteBlock2) && (
            <>
              <table className="govuk-table">
                <thead>
                  <tr>
                    <th>Footnote</th>
                    <th>Subjects</th>
                    <th>Indicators</th>
                    <th>Filters</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody className="govuk-body-s">
                  <tr>
                    <td className="govuk-body-s">{valueFootnote}</td>
                    <td>
                      <ul className="govuk-list--bullet">
                        <li>Geographical absence</li>
                      </ul>
                    </td>
                    <td>
                      <ul className="govuk-list--bullet">
                        <li>Authorised absence rate</li>
                        <li>Overall absence rate</li>
                        <li>Unauthorised absence rate</li>
                      </ul>
                    </td>
                    <td>
                      <ul className="govuk-list--bullet">
                        <li>All pupils</li>
                        <li>All schools</li>
                      </ul>
                    </td>
                    <td>
                      <button
                        className="govuk-button govuk-!-margin-right-3 govuk-!-margin-bottom-0"
                        type="submit"
                        onClick={() => setEditFootnoteBlock(false)}
                      >
                        Edit
                      </button>
                      <button
                        className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
                        type="submit"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                  {editFootnoteBlock2 && (
                    <tr>
                      <td className="govuk-body-s">{valueFootnote2}</td>
                      <td>
                        <ul className="govuk-list--bullet">
                          <li>Geographical absence</li>
                          <li>Local authority</li>
                        </ul>
                      </td>
                      <td>
                        <ul className="govuk-list--bullet">
                          <li>All</li>
                        </ul>
                      </td>
                      <td>
                        <ul className="govuk-list--bullet">
                          <li>All</li>
                        </ul>
                      </td>
                      <td>
                        <button
                          className="govuk-button govuk-!-margin-right-3 govuk-!-margin-bottom-0"
                          type="submit"
                          onClick={() => setEditFootnoteBlock(false)}
                        >
                          Edit
                        </button>
                        <button
                          className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
                          type="submit"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>

              <button
                className="govuk-button govuk-!-margin-right-3"
                type="submit"
                onClick={() => setAddAnotherFootnote(true)}
              >
                Add another footnote
              </button>
            </>
          )}
        </>
      )}

      {addAnotherFootnote && (
        <>
          <p>Select either one or multiple subject areas from below</p>
          <Accordion id="uploaded-data">
            <AccordionSection heading="Geographical absence">
              <PrototypeFootnotes />
            </AccordionSection>
            <AccordionSection heading="Local authority">
              <PrototypeFootnotes />
            </AccordionSection>
            <AccordionSection heading="National characteristics">
              <PrototypeFootnotes />
              <button className="govuk-button" type="button">
                Add footnote
              </button>
            </AccordionSection>
          </Accordion>
          <h3 className="govuk-heading-s">Footnote</h3>

          <textarea
            className="govuk-textarea govuk-!-margin-bottom-3"
            id="footnote-2"
            name="footnote-2"
            rows={5}
            value={valueFootnote2}
            onChange={event => {
              setValueFootnote2(event.target.value);
            }}
          >
            test
          </textarea>
          <button
            className="govuk-button"
            type="submit"
            onClick={() => {
              setEditFootnoteBlock2(true);
              setAddAnotherFootnote(false);
            }}
          >
            Save
          </button>
        </>
      )}
      {!addNewFootnote && (
        <>
          <p>The are currently no footnotes related to the data uploads</p>
          <button
            className="govuk-button govuk-!-margin-right-3"
            type="submit"
            onClick={() => {
              setAddNewFootnote(true);
            }}
          >
            Add footnote
          </button>
        </>
      )}
    </>
  );
};

export default PrototypeAddFootnotes;
