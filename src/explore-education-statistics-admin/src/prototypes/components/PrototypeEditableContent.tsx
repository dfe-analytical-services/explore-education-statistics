import * as React from "react";
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
// @ts-ignore
import Editor from '@ckeditor/ckeditor5-build-balloon-block';
import {ReactComponentLike, ReactNodeLike} from "prop-types";

// import 'react-draft-wysiwyg/dist/react-draft-wysiwyg.css';

interface Props {
  content: string;
}
export class PrototypeEditableContent extends React.Component<Props> {

  public render() {

    const elementstring : string = this.props.content;

    return (
      <CKEditor
        editor={Editor}
        data={ elementstring }
      >
        {this.props.children}
      </CKEditor>
    );
  }

}