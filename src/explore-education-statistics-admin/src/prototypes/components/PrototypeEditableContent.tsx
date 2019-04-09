import * as React from 'react';
// @ts-ignore
import CKEditor from '@ckeditor/ckeditor5-react';
// @ts-ignore
import Editor from '@ckeditor/ckeditor5-build-balloon-block';
// @ts-ignore
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { Fragment } from 'react';

// import 'react-draft-wysiwyg/dist/react-draft-wysiwyg.css';

interface Props {
  content: string;
}

interface State {
  content: string;
  editing: boolean;
  unsaved: boolean;
}

export class PrototypeEditableContent extends React.Component<Props, State> {
  private ref: HTMLElement | null = null;

  private temporaryContent: string = '';

  public state: State = {
    content: '',
    editing: false,
    unsaved: false,
  };

  componentDidMount() {

    this.setState({ content: this.props.content, editing: false, unsaved: false });

    this.temporaryContent = this.props.content;

    if (!this.state.editing && this.ref) {
      this.ref.innerHTML = this.state.content;
    }
  }

  componentDidUpdate() {
    if (!this.state.editing && this.ref) {
      this.ref.innerHTML = this.state.content;
    }
  }

  setEditing = (event: React.MouseEvent<HTMLElement>) => {
    if (!this.state.editing) {
      this.setState({ editing: true });
    }
  };

  save = () => {
    this.setState({
      editing: false,
      unsaved: true,
      content: this.temporaryContent,
    });
  };

  public render() {
    return (
      <Fragment>
        {this.state.editing ? (
          <div className="editable-content editable-content-editing">
            <div className="editable-button">
              <button onClick={this.save}>Save</button>
            </div>
            <CKEditor
              editor={ClassicEditor}
              data={this.state.content}
              onChange={(event: any, editor: any) => {
                this.temporaryContent = editor.getData();
              }}
              onInit={(editor: any) => {
                editor.editing.view.focus();
              }}
            />
          </div>
        ) : (
          <div
            className="editable-content"
            onClick={event => this.setEditing(event)}
          >
            {this.state.unsaved ? (
              <div className="editable-button unsaved">Click to edit</div>
            ) : (
              <div className="editable-button">Click to edit</div>
            )}
            <div ref={ref => (this.ref = ref)} />
          </div>
        )}
      </Fragment>
    );
  }
}
