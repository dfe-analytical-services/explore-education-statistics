import React from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';

import styles from './PrototypeEditableContentAddComment.module.scss';

const ContentAddComment = () => {
  return (
    <>
      <div className={styles.addComment}>
        <Details summary="Add comment" className="govuk-!-margin-bottom-1">
          <form>
            <textarea name="comment" id="comment" />
            <button type="submit" className="govuk-button">
              Submit
            </button>
          </form>
        </Details>
      </div>
    </>
  );
};

export default ContentAddComment;
