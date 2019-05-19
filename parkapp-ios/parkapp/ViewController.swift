//
//  ViewController.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

class ViewController: UITableViewController {
  @IBOutlet weak var vacantMarker: UIView!
  @IBOutlet weak var occupiedMarker: UIView!
  @IBOutlet weak var vacantLabel: UILabel!
  @IBOutlet weak var occupiedLabel: UILabel!
  @IBOutlet weak var totalLabel: UILabel!
  @IBOutlet weak var notifyButton: UIButton!
  @IBOutlet weak var notifyHint: UILabel!

  private var diffObserver: NSObjectProtocol?

  var parking: Parking!

  override func viewDidLoad() {
    super.viewDidLoad()
    vacantMarker.layer.cornerRadius = vacantMarker.bounds.height/2.0
    occupiedMarker.layer.cornerRadius = occupiedMarker.bounds.height/2.0

    update()

    diffObserver =
      NotificationCenter.default
        .addObserver(forName: CamsRepo.diff,
                     object: CamsRepo.shared,
                     queue: OperationQueue.main) { [unowned self] (ntf) in
                      guard let diffIds = ntf.userInfo?["ids"] as? [Int] else { return }
                      if diffIds.contains(self.parking.number) {
                        self.update()
                      }
    }
  }

  deinit {
    NotificationCenter.default.removeObserver(diffObserver)
  }

  private func update() {
    let currentId = parking.number
    parking = CamsRepo.shared.list.first(where: { $0.number == currentId })
    title = parking.title
    vacantLabel.text = String(format: NSLocalizedString("places_vacant", comment: ""), parking.vacant)
    occupiedLabel.text = String(format: NSLocalizedString("places_occupied", comment: ""), parking.occupied)
    totalLabel.text = String(format: NSLocalizedString("places_total", comment: ""), parking.total)
    if parking.vacant > 0 {
      notifyButton.isEnabled = false
      notifyHint.isEnabled = false
      CamsRepo.shared.unsubscribe(camId: currentId)
    } else {
      notifyButton.isEnabled = true
      notifyHint.isEnabled = true
    }

    var title = "Уведомлять о свободных местах"
    if CamsRepo.shared.notifyIds.contains(parking.number) {
      title = "Отписаться от уведомлений"
    }

    notifyButton.setTitle(title, for: .normal)
  }

  @IBAction func notifyVacant(_ sender: Any) {
    if CamsRepo.shared.notifyIds.contains(parking.number) {
      CamsRepo.shared.unsubscribe(camId: parking.number)
    } else {
      CamsRepo.shared.subscribe(camId: parking.number)
    }
    update()
  }

  override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
    if segue.identifier == "showCam" {
      guard let camVc = segue.destination as? CamViewController else { return }
      camVc.camId = parking.number
    }
  }

  func setupStandalone() {
    navigationItem.leftBarButtonItem = UIBarButtonItem(barButtonSystemItem: .done,
                                                       target: self,
                                                       action: #selector(close))
  }

  @objc func close() {
    dismiss(animated: true, completion: nil)
  }


}

