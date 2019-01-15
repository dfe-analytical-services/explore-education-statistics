import React, { Component } from "react";
import api from "../Api";

export default class Upload extends Component {
  constructor(props) {
    super(props);
    this.state = { selectedFile: null, publication: "", loaded: 0 };
  }

  handleSelectedFile = event => {
    this.setState({
      selectedFile: event.target.files[0],
      loaded: 0
    });
  };

  handleChange = event => {
    this.setState({ publication: event.target.value });
  }

  handleUpload = () => {
    const data = new FormData();
    data.append("file", this.state.selectedFile, this.state.selectedFile.name);

    api
      .post(`Upload/Upload/${this.state.publication}`, data, {
        onUploadProgress: ProgressEvent => {
          this.setState({
            loaded: (ProgressEvent.loaded / ProgressEvent.total) * 100
          });
        }
      })
      .then(res => {
        console.log(res.statusText);
      });
  };

  render() {
    return (
      <div>
        <label for="publicationId">Publication guid:</label> 
        <input
          id="publicationId"
          type="text"
          value={this.state.publication}
          onChange={this.handleChange}
        />
        <input type="file" name="" id="" onChange={this.handleSelectedFile} />
        <button onClick={this.handleUpload}>Upload</button>
        <div> {Math.round(this.state.loaded, 2)} %</div>
      </div>
    );
  }
}
