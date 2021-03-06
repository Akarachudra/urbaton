//
//  CamViewController.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

class CamViewController: UIViewController {

  enum ProcessErrors: Error {
    case noData
    case notAnImage
  }

  @IBOutlet weak var scrollView: UIScrollView!

  var camId: Int!
  private var zoomingDelegate: ZoomingDelegate?
  private var diffObserver: NSObjectProtocol?

  override func viewDidLoad() {
    super.viewDidLoad()

    diffObserver =
      NotificationCenter.default
        .addObserver(forName: CamsRepo.diff,
                     object: CamsRepo.shared,
                     queue: OperationQueue.main) { [unowned self] (ntf) in
                      guard let diffIds = ntf.userInfo?["ids"] as? [Int] else { return }
                      if diffIds.contains(self.camId) {
                        self.loadImage()
                      }
    }

    self.loadImage()
  }

  override func viewWillDisappear(_ animated: Bool) {
    super.viewWillDisappear(animated)
    NotificationCenter.default.removeObserver(diffObserver!)
  }

  func loadImage() {
    let camImageURL = Constants.baseURL.appendingPathComponent("camera/file/\(camId!)")
    URLSession.shared.dataTask(with: camImageURL) { (data, response, error) in
      if let error = error {
        self.showFailedLoad(error)
        return
      }
      guard let data = data else {
        self.showFailedLoad(ProcessErrors.noData)
        return
      }

      guard let image = UIImage(data: data) else {
        self.showFailedLoad(ProcessErrors.notAnImage)
        return
      }

      self.placeImage(image: image)
    }.resume()
  }

  func showFailedLoad(_ error: Error) {
    DispatchQueue.main.async {

    }
  }

  private var imageView: UIImageView?

  func placeImage(image: UIImage) {
    DispatchQueue.main.async {
      if self.imageView == nil {
        self.imageView = UIImageView(image: image)
      } else {
        self.imageView?.image = image
      }
      self.zoomingDelegate = ZoomingDelegate(view: self.imageView!)
      self.scrollView.delegate = self.zoomingDelegate
      self.scrollView.addSubview(self.imageView!)
      self.scrollView.zoomScale = 0.6
    }
  }

  private var feedbackView: UITextView?

  @IBAction func addFeedback(_ sender: Any) {
    let width = UIScreen.main.bounds.width * 0.9
    let xPad = UIScreen.main.bounds.width * 0.05
    let messageBubble = UIView(frame: CGRect(x: xPad, y: 150, width: width, height: 260))
    messageBubble.backgroundColor = .white
    messageBubble.layer.cornerRadius = 8
    messageBubble.layer.borderColor = UIColor.gray.cgColor
    messageBubble.layer.borderWidth = 1.0
    let textView = UITextView()
    feedbackView = textView
    textView.font = UIFont.systemFont(ofSize: 17.0)
    textView.translatesAutoresizingMaskIntoConstraints = false
    let toolbar = UIToolbar(frame: CGRect(x: 0, y: 0, width: view.bounds.width, height: 44))
    toolbar.items = [
      UIBarButtonItem(barButtonSystemItem: .cancel, target: self, action: #selector(cancelFeedback)),
      UIBarButtonItem(barButtonSystemItem: .flexibleSpace, target: nil, action: nil),
      UIBarButtonItem(barButtonSystemItem: .reply, target: self, action: #selector(sendFeedback))]
    textView.inputAccessoryView = toolbar
    messageBubble.addSubview(textView)
    NSLayoutConstraint.activate([
      textView.leadingAnchor.constraint(equalTo: messageBubble.leadingAnchor, constant: 12),
      textView.trailingAnchor.constraint(equalTo: messageBubble.trailingAnchor, constant: -12),
      textView.topAnchor.constraint(equalTo: messageBubble.topAnchor, constant: 8),
      textView.bottomAnchor.constraint(equalTo: messageBubble.bottomAnchor, constant: -8)
      ])

    messageBubble.alpha = 0.0
    view.addSubview(messageBubble)

    UIView.animate(withDuration: 0.4, animations: {
      messageBubble.alpha = 1.0
    }, completion: { _ in
      textView.becomeFirstResponder()
    })
  }

  @objc func cancelFeedback() {
    feedbackView?.superview?.removeFromSuperview()
  }

  struct Feedback: Encodable {
    let title: String
    let text: String
    let cameraNumber: Int
    let x: Int?
    let y: Int?
  }

  @objc private func sendFeedback() {
    guard let message = feedbackView?.text else {
      return
    }
    feedbackView?.superview?.removeFromSuperview()

    var request = URLRequest(url: Constants.baseURL.appendingPathComponent("feedback"))
    request.httpMethod = "POST"
    let formatter = DateFormatter()
    formatter.dateStyle = .short
    formatter.timeStyle = .short
    let feedback = Feedback(title: "Отзыв \(formatter.string(from: Date()))",
      text: message,
      cameraNumber: camId, x: 0, y: 0)
    request.httpBody = try? JSONEncoder().encode(feedback)
    request.addValue("application/json", forHTTPHeaderField: "Content-Type")
    URLSession.shared.dataTask(with: request).resume()
  }

  class ZoomingDelegate: NSObject, UIScrollViewDelegate {
    weak var imageView: UIImageView?

    init(view: UIImageView) {
      imageView = view
    }

    func viewForZooming(in scrollView: UIScrollView) -> UIView? {
      return imageView
    }
  }
}
