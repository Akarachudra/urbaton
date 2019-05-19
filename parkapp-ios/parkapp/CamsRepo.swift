//
//  CamsRepo.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

struct Parking: Decodable, Equatable {
  let number: Int
  let title: String
  let total: Int
  let vacant: Int
  let occupied: Int

  enum CodingKeys: String, CodingKey {
    case number = "CameraNumber"
    case title = "Description"
    case total = "TotalPlaces"
    case vacant = "FreePlaces"
    case occupied = "OccupiedPlaces"
  }

  static func ==(lhs: Parking, rhs: Parking) -> Bool {
    guard lhs.number == rhs.number else { return false }
    return lhs.vacant == rhs.vacant && lhs.occupied == rhs.occupied
  }
}

class CamsRepo: NSObject {
  static let shared = CamsRepo()

  private var timer: Timer?
  func startPolling() {
    timer = Timer.scheduledTimer(withTimeInterval: 1, repeats: true, block: { [unowned self] (_) in
      self.loadCams()
    })
  }

  private(set) var list = [Parking]()
  static let allLoaded = Notification.Name("repo_load_completed")
  static let diff = Notification.Name("repo_diff")
  static let failedLoad = Notification.Name("repo_failed")

  func refresh() {
    loadCams()
  }

  private func loadCams() {
    let cameras = Constants.baseURL.appendingPathComponent("info")
    URLSession.shared.dataTask(with: cameras) { (data, response, error) in
      defer {
        self.postLoaded()
      }

      if error != nil {
        self.postFailed()
        return
      }

      guard let data = data else {
        self.postFailed()
        return
      }

      if let listItems = try? JSONDecoder().decode([Parking].self, from: data) {
        self.diff(current: self.list, new: listItems)
        self.list = listItems
      } else {
        self.postFailed()
      }
      }.resume()
  }

  private func diff(current: [Parking], new: [Parking]) {
    guard current.count == new.count else {
      postDiff(camIds: new.map({ $0.number }))
      return
    }
    var diff = [Parking]()
    for (index, _) in current.enumerated() {
      guard current[index].number == new[index].number else { continue }
      if current[index] != new[index] {
        diff.append(new[index])
      }
    }
  }

  private func postLoaded() {
    NotificationCenter.default.post(name: CamsRepo.allLoaded, object: self)
  }

  private func postDiff(camIds: [Int]) {
    NotificationCenter.default.post(name: CamsRepo.diff, object: self, userInfo: ["ids": camIds])
  }

  private func postFailed() {
    NotificationCenter.default.post(name: CamsRepo.failedLoad, object: self, userInfo: nil)
  }

  deinit {
    timer?.invalidate()
    timer = nil
  }
}
